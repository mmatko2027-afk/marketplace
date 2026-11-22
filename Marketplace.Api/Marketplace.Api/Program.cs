using System.Linq;
using System.Text;
using System.Reflection;

using Marketplace.Application.Files;
using Marketplace.Application.Background;
using Marketplace.Application.Integrations;
using Marketplace.Application.AI;
using Marketplace.Application.Delivery;
using Marketplace.Application.Repositories;
using Marketplace.Application.Services;

using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.Repositories;
using Marketplace.Infrastructure.Files;
using Marketplace.Infrastructure.Background;
using Marketplace.Infrastructure.Integrations;
using Marketplace.Infrastructure.AI;
using Marketplace.Infrastructure.Delivery;

using Marketplace.Domain.Entities;
using Marketplace.Api.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<AiOptions>(
    builder.Configuration.GetSection("Ai"));

builder.Services.Configure<StoreRulesOptions>(
    builder.Configuration.GetSection("StoreRules"));

builder.Services.AddHttpClient<IAiSupportService, OpenAiCompatibleSupportService>(
    (serviceProvider, client) =>
    {
        var settings = serviceProvider
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>()
            .Value;

        if (Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var uri))
            client.BaseAddress = uri;

        client.Timeout = TimeSpan.FromSeconds(
            Math.Max(1, settings.TimeoutSeconds));
    });

builder.Services.Configure<NovaPoshtaOptions>(
    builder.Configuration.GetSection("NovaPoshta"));

builder.Services.AddHttpClient<INovaPoshtaService, NovaPoshtaService>(
    (serviceProvider, client) =>
    {
        var settings = serviceProvider
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<NovaPoshtaOptions>>()
            .Value;

        if (Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var uri))
            client.BaseAddress = uri;

        client.Timeout = TimeSpan.FromSeconds(
            Math.Max(1, settings.TimeoutSeconds));
    });

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IJobStatusStore, InMemoryJobStatusStore>();

builder.Services.AddHttpClient<IExternalCatalogClient, ExternalCatalogClient>(
    client =>
    {
        var baseAddress =
            builder.Configuration["ExternalServices:CatalogBaseUrl"];

        if (Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri))
            client.BaseAddress = uri;

        client.Timeout = TimeSpan.FromSeconds(
            builder.Configuration.GetValue(
                "ExternalServices:TimeoutSeconds", 5));
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ClickBack API",
        Version = "v1",
        Description = "API електронного маркетплейсу ClickBack"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введіть JWT токен"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

    options.IncludeXmlComments(Path.Combine(
        AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

builder.Services.AddProblemDetails();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
builder.Services.AddScoped<FavoriteService>();

builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();
builder.Services.AddScoped<MessageService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
var jwtKey = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT SecretKey не налаштовано.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer не налаштовано.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience не налаштовано.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' не налаштовано.");

builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IProductConditionRepository, ProductConditionRepository>();

builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<BrandService>();
builder.Services.AddScoped<ProductConditionService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<IListingImageRepository, ListingImageRepository>();
builder.Services.AddScoped<ListingImageService>();

builder.Services.AddSingleton<IFileStorage>(
    new LocalFileStorage(builder.Environment.ContentRootPath));

builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ReviewService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<MarketplaceDbContext>();

    db.Database.Migrate();

    var adminEmail = builder.Configuration["Admin:Email"];
    var adminPassword = builder.Configuration["Admin:Password"];

    if (string.IsNullOrWhiteSpace(adminEmail) ||
        string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "Email або пароль адміністратора не налаштовано.");
    }

    adminEmail = adminEmail.Trim().ToLowerInvariant();

    var existingAdmin = db.Users
        .FirstOrDefault(user => user.Email == adminEmail);

    if (existingAdmin == null)
    {
        var admin = new User
        {
            Name = "Admin",
            Email = adminEmail,
            Role = "Admin"
        };

        var passwordHasher = new PasswordHasher<User>();

        admin.PasswordHash = passwordHasher.HashPassword(
            admin,
            adminPassword);

        db.Users.Add(admin);
        db.SaveChanges();
    }
    else if (existingAdmin.Role != "Admin")
    {
        throw new InvalidOperationException(
            "Вказаний email уже належить користувачу без ролі Admin.");
    }
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy"
}));

app.Run();

public partial class Program
{
}
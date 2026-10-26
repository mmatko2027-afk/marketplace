using System.Linq;
using Marketplace.Application.Repositories;
using Marketplace.Application.Services;
using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();
builder.Services.AddScoped<MessageService>();
var databasePath = Path.Combine(
    builder.Environment.ContentRootPath,
    "marketplace.db");

var connectionString =
    @"Data Source=C:\Users\Lenovo\Documents\marketplace\Marketplace.Api\Marketplace.Api\marketplace.db";

builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();


builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<CategoryService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<MarketplaceDbContext>();

    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Name = "Test Seller",
            Email = "seller@example.com",
            PasswordHash = "test-hash"
        });

        db.SaveChanges();
    }
}



app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy"
}));

app.Run();

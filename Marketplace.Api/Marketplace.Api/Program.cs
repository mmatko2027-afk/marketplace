using Marketplace.Application.Repositories;
using Marketplace.Application.Services;
using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlite("Data Source=marketplace.db"));

builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<ListingService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy"
}));

app.MapControllers();

app.Run();
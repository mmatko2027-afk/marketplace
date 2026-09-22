using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Data;

public class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Listing> Listings => Set<Listing>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<Message> Messages => Set<Message>();
}
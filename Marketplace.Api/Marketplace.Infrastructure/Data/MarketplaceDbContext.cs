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
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductCondition> ProductConditions => Set<ProductCondition>();
    public DbSet<ListingSpecification> ListingSpecifications => Set<ListingSpecification>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Listing>()
            .HasOne(l => l.Category)
            .WithMany(c => c.Listings)
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Listing>()
            .HasIndex(l => new { l.Status, l.CreatedAt });

        modelBuilder.Entity<Listing>()
            .HasOne(l => l.Brand)
            .WithMany(b => b.Listings)
            .HasForeignKey(l => l.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Listing>()
            .HasOne(l => l.ProductCondition)
            .WithMany(c => c.Listings)
            .HasForeignKey(l => l.ProductConditionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ListingSpecification>()
            .HasOne(s => s.Listing)
            .WithMany(l => l.Specifications)
            .HasForeignKey(s => s.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ListingImage>()
            .HasOne(i => i.Listing)
            .WithMany(l => l.Images)
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Brand>()
            .HasIndex(b => b.Name)
            .IsUnique();

        modelBuilder.Entity<ProductCondition>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<ListingSpecification>()
            .HasIndex(s => new { s.ListingId, s.Name });

        modelBuilder.Entity<Listing>()
            .HasOne(l => l.Seller)
            .WithMany(u => u.Listings)
            .HasForeignKey(l => l.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Listing)
            .WithMany(l => l.Favorites)
            .HasForeignKey(f => f.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.ListingId })
            .IsUnique();

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Listing)
            .WithMany()
            .HasForeignKey(m => m.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Listing>()
            .Property(l => l.Price)
            .HasPrecision(12, 2);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Buyer)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(12, 2);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Listing)
            .WithMany(l => l.OrderItems)
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(12, 2);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Order)
            .WithMany(o => o.Reviews)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Listing)
            .WithMany(l => l.Reviews)
            .HasForeignKey(r => r.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Buyer)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.OrderId, r.ListingId })
            .IsUnique();

        modelBuilder.Entity<Review>()
            .Property(r => r.Rating)
            .IsRequired();
    }
}
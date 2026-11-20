namespace Marketplace.Domain.Entities;

public class Listing
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string Model { get; set; } = "";

    public decimal Price { get; set; }

    public string Status { get; set; } = "active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? BrandId { get; set; }

    public Brand? Brand { get; set; }

    public int? ProductConditionId { get; set; }

    public ProductCondition? ProductCondition { get; set; }

    public int? ProductionYear { get; set; }

    public string Warranty { get; set; } = "";

    public string Defects { get; set; } = "";

    public string PackageContents { get; set; } = "";

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public int SellerId { get; set; }

    public User? Seller { get; set; }

    public List<Favorite> Favorites { get; set; } = new();

    public List<ListingSpecification> Specifications { get; set; } = new();

    public List<ListingImage> Images { get; set; } = new();

    public List<OrderItem> OrderItems { get; set; } = new();

    public List<Review> Reviews { get; set; } = new();
}
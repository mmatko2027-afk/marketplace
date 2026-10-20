namespace Marketplace.Domain.Entities;

public class Listing
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public decimal Price { get; set; }

    public string Status { get; set; } = "active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public int SellerId { get; set; }

    public User? Seller { get; set; }

    public List<Favorite> Favorites { get; set; } = new();
}
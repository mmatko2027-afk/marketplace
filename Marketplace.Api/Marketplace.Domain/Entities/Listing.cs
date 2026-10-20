namespace Marketplace.Domain.Entities;

public class Listing
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Status { get; set; } = "active";

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public int SellerId { get; set; }

    public User? Seller { get; set; }
}
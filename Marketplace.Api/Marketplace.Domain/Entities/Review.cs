namespace Marketplace.Domain.Entities;

public class Review
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }

    public int BuyerId { get; set; }

    public User? Buyer { get; set; }

    public int Rating { get; set; }

    public string Text { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

namespace Marketplace.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public int BuyerId { get; set; }

    public User? Buyer { get; set; }

    public string Status { get; set; } = "Pending";

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();

    public List<Review> Reviews { get; set; } = new();
}

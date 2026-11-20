namespace Marketplace.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }

    public decimal UnitPrice { get; set; }

    public string TitleSnapshot { get; set; } = "";
}

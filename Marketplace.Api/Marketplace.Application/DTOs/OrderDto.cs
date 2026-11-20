namespace Marketplace.Application.DTOs;

public class OrderDto
{
    public int Id { get; init; }
    public int BuyerId { get; init; }
    public string Status { get; init; } = "";
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyCollection<OrderItemDto> Items { get; init; } = [];
}

public class OrderItemDto
{
    public int ListingId { get; init; }
    public string Title { get; init; } = "";
    public decimal UnitPrice { get; init; }
}

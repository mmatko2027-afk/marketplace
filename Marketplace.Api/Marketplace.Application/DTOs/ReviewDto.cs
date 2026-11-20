namespace Marketplace.Application.DTOs;

public class ReviewDto
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public int ListingId { get; init; }
    public int BuyerId { get; init; }
    public string BuyerName { get; init; } = "";
    public int Rating { get; init; }
    public string Text { get; init; } = "";
    public DateTime CreatedAt { get; init; }
}

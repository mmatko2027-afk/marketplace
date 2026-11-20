namespace Marketplace.Application.DTOs;

public class MessageDto
{
    public int Id { get; init; }
    public string Text { get; init; } = "";
    public DateTime SentAt { get; init; }
    public int SenderId { get; init; }
    public string SenderName { get; init; } = "";
    public int ReceiverId { get; init; }
    public string ReceiverName { get; init; } = "";
    public int ListingId { get; init; }
}

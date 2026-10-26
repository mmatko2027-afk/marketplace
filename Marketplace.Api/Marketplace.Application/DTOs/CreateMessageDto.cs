
namespace Marketplace.Application.DTOs;

public class CreateMessageDto
{
    public string Text { get; set; } = "";

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public int ListingId { get; set; }
}

namespace Marketplace.Domain.Entities;

public class Message
{
    public int Id { get; set; }

    public string Text { get; set; } = "";

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public int SenderId { get; set; }

    public User? Sender { get; set; }

    public int ReceiverId { get; set; }

    public User? Receiver { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }
}
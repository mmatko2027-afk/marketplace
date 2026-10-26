namespace Marketplace.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Listing> Listings { get; set; } = new();

    public List<Favorite> Favorites { get; set; } = new();

    public List<Message> SentMessages { get; set; } = new();

    public List<Message> ReceivedMessages { get; set; } = new();
}
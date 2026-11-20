namespace Marketplace.Domain.Entities;

public class ProductCondition
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public List<Listing> Listings { get; set; } = new();
}

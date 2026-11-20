namespace Marketplace.Domain.Entities;

public class ListingSpecification
{
    public int Id { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}

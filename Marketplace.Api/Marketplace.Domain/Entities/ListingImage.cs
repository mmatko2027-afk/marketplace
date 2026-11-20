namespace Marketplace.Domain.Entities;

public class ListingImage
{
    public int Id { get; set; }

    public int ListingId { get; set; }

    public Listing? Listing { get; set; }

    public string FilePath { get; set; } = "";

    public int SortOrder { get; set; }
}

namespace Marketplace.Application.DTOs;

public class ListingDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Model { get; set; } = "";
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int? BrandId { get; set; }
    public string BrandName { get; set; } = "";
    public int? ProductConditionId { get; set; }
    public string ProductConditionName { get; set; } = "";
    public int? ProductionYear { get; set; }
    public string Warranty { get; set; } = "";
    public string Defects { get; set; } = "";
    public string PackageContents { get; set; } = "";
    public IReadOnlyCollection<ListingSpecificationDto> Specifications { get; set; } = [];
    public string Status { get; set; } = "active";
}
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class CreateListingDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = "";

    [StringLength(1000)]
    public string Description { get; set; } = "";

    [StringLength(150)]
    public string Model { get; set; } = "";

    [Range(0.01, 100000000)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int? ProductConditionId { get; set; }

    [Range(1900, 2100)]
    public int? ProductionYear { get; set; }

    [StringLength(500)]
    public string Warranty { get; set; } = "";

    [StringLength(1000)]
    public string Defects { get; set; } = "";

    [StringLength(1000)]
    public string PackageContents { get; set; } = "";

    public List<CreateListingSpecificationDto> Specifications { get; set; } = [];

    [RegularExpression("active|reserved|sold", ErrorMessage = "Недопустимий статус оголошення.")]
    public string Status { get; set; } = "active";
}
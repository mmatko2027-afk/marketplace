using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class CreateListingDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = "";

    [StringLength(1000)]
    public string Description { get; set; } = "";

    [Range(0.01, 100000000)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public string Status { get; set; } = "active";
    public int SellerId { get; set; }
}
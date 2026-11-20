using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class CreateListingSpecificationDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Value { get; set; } = "";
}

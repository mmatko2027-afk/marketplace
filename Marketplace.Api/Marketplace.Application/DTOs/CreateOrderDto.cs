using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class CreateOrderDto
{
    [Required]
    [MinLength(1)]
    public List<int> ListingIds { get; set; } = [];
}

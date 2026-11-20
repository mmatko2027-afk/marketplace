using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class UpdateOrderStatusDto
{
    [Required]
    [RegularExpression("Pending|Confirmed|Shipped|Delivered|Cancelled")]
    public string Status { get; set; } = "Pending";
}

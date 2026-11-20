using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class UpdateMessageDto
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Text { get; set; } = "";
}

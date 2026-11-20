
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class CreateMessageDto
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Text { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int SenderId { get; set; }

    [Range(1, int.MaxValue)]
    public int ReceiverId { get; set; }

    [Range(1, int.MaxValue)]
    public int ListingId { get; set; }
}

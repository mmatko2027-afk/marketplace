using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class CreateReviewDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Text { get; set; } = "";
}

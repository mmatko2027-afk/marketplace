using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class CreateCategoryDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";
}
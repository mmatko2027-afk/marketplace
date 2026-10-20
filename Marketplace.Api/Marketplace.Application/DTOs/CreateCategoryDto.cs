using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Введіть назву категорії.")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

}
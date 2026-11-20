using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs;

public class UpdateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [RegularExpression("Buyer|Admin", ErrorMessage = "Недопустима роль користувача.")]
    public string Role { get; set; } = "Buyer";
}

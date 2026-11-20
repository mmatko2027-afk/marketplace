using System.ComponentModel.DataAnnotations;
using Marketplace.Application.Validation;

namespace Marketplace.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [Password]
    public string Password { get; set; } = "";
}
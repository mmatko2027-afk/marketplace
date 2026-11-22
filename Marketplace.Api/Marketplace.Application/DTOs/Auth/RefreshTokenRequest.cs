using System.ComponentModel.DataAnnotations;

namespace Marketplace.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = "";
}
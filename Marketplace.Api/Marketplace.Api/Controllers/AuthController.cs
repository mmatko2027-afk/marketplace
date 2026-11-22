
using Marketplace.Api.Services;
using Marketplace.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    /// <summary>Забезпечує реєстрацію та автентифікацію користувачів із використанням JWT-токенів.</summary>
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Реєструє нового користувача.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result == null)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Реєстрацію не виконано",
                detail: "Перевірте введені дані або переконайтеся, що Email ще не використовується.");

        return Ok(result);
    }
    /// <summary>Виконує вхід користувача в систему.</summary>

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized,
                title: "Автентифікація не виконана",
                detail: "Неправильний Email або пароль.");

        return Ok(result);
    }

    /// <summary>Оновлює access-токен за допомогою refresh-токена.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);
        if (result == null)
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Оновлення токена не виконано",
                detail: "Refresh-токен недійсний, прострочений або відкликаний.");

        return Ok(result);
    }

    /// <summary>Виконує вихід із системи та відкликає refresh-токен.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        int? currentUserId = null;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var parsedId))
            currentUserId = parsedId;

        var revoked = await _authService.LogoutAsync(request.RefreshToken, currentUserId);
        if (!revoked)
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Вихід не виконано",
                detail: "Токен не знайдено або він уже відкликаний.");

        return NoContent();
    }
}

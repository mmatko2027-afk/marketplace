
using Marketplace.Api.Services;
using Marketplace.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

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
}

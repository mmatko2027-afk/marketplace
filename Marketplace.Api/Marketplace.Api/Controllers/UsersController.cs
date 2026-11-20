
using Marketplace.Application.DTOs;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }
    /// <summary>Отримує список усіх користувачів. Доступно лише адміністратору.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetAllAsync();

        var result = users.Select(user => new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.CreatedAt
        });

        return Ok(result);
    }
    /// <summary>Отримує інформацію про користувача за його ідентифікатором.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        if (!IsAdmin() && GetCurrentUserId() != id)
            return Forbid();

        return Ok(ToDto(user));
    }
    /// <summary>Оновлює дані користувача. Доступно лише адміністратору.</summary>

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        var updated = await _service.UpdateAsync(
            id, dto.Name.Trim(), dto.Email.Trim().ToLowerInvariant(), dto.Role);

        if (!updated)
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Користувача не оновлено",
                detail: "Користувача не знайдено або Email уже використовується.");

        return NoContent();
    }
    /// <summary>Видаляє користувача за ідентифікатором. Доступно лише адміністратору.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static object ToDto(Marketplace.Domain.Entities.User user)
    {
        return new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.CreatedAt
        };
    }

    private bool IsAdmin() => User.IsInRole("Admin");

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}


using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    /// <summary>Забезпечує керування списком обраних товарів користувача: перегляд, додавання та видалення оголошень із перевіркою прав доступу.</summary>
    private readonly FavoriteService _service;

    public FavoritesController(FavoriteService service)
    {
        _service = service;
    }
    /// <summary>Отримує список обраних товарів користувача. Доступно лише власнику списку.</summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        if (GetCurrentUserId() != userId)
            return Forbid();

        var favorites = await _service.GetByUserAsync(userId);

        return Ok(favorites);
    }
    /// <summary>Додає оголошення до списку обраних товарів користувача. Доступно лише власнику списку.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromQuery] int userId,
        [FromQuery] int listingId)
    {
        if (GetCurrentUserId() != userId)
            return Forbid();

        var created = await _service.AddAsync(userId, listingId);

        if (!created)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Товар не додано до обраного",
                detail: "Користувача або оголошення не знайдено, або товар уже в обраному.");
        }

        return Ok(new { message = "Товар додано до обраного." });
    }
    /// <summary>Видаляє оголошення зі списку обраних товарів користувача. Доступно лише власнику списку.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var favorite = await _service.GetByIdAsync(id);
        if (favorite is null)
            return NotFound();

        if (GetCurrentUserId() != favorite.UserId)
            return Forbid();

        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}


using Marketplace.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly FavoriteService _service;

    public FavoritesController(FavoriteService service)
    {
        _service = service;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var favorites = await _service.GetByUserAsync(userId);

        return Ok(favorites);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        int userId,
        int listingId)
    {
        var created = await _service.AddAsync(userId, listingId);

        if (!created)
        {
            return BadRequest(
                "Користувача або оголошення не знайдено, або товар уже в обраному.");
        }

        return Ok(new { message = "Товар додано до обраного." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

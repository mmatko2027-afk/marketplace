using System.Security.Claims;
using Marketplace.Application.DTOs;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ReviewService _service;

    public ReviewsController(ReviewService service) => _service = service;
    /// <summary>Забезпечує створення, перегляд, редагування та видалення відгуків користувачів маркетплейсу з перевіркою прав доступу.</summary>
    [HttpGet("listing/{listingId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByListing(int listingId)
    {
        var reviews = await _service.GetByListingAsync(listingId);
        return Ok(new
        {
            items = reviews.Select(ToDto),
            averageRating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating)
        });
    }
    /// <summary>Отримує інформацію про відгук за його ідентифікатором. Доступно всім користувачам.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var review = await _service.GetByIdAsync(id);
        return review is null ? NotFound() : Ok(ToDto(review));
    }
    /// <summary>Створює новий відгук для замовлення та оголошення. Доступно лише покупцю, який завершив замовлення.</summary>
    [HttpPost("order/{orderId}/listing/{listingId}")]
    [Authorize]
    public async Task<IActionResult> Create(int orderId, int listingId, CreateReviewDto dto)
    {
        var buyerId = GetCurrentUserId();
        if (buyerId is null)
            return Unauthorized();

        var review = await _service.CreateAsync(
            buyerId.Value, orderId, listingId, dto.Rating, dto.Text);

        if (review is null)
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Відгук не створено",
                detail: "Замовлення має бути Delivered, належати користувачу і не мати повторного відгуку для цього товару.");

        return CreatedAtAction(nameof(GetById), new { id = review.Id }, ToDto(review));
    }
    /// <summary>Оновлює відгук. Доступно лише покупцю, який створив відгук.</summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, CreateReviewDto dto)
    {
        var review = await _service.GetByIdAsync(id);
        if (review is null)
            return NotFound();
        if (review.BuyerId != GetCurrentUserId())
            return Forbid();

        return await _service.UpdateAsync(id, dto.Rating, dto.Text)
            ? NoContent()
            : NotFound();
    }
    /// <summary>Видаляє відгук за його ідентифікатором. Доступно лише покупцю, який створив відгук або адміністратору.</summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _service.GetByIdAsync(id);
        if (review is null)
            return NotFound();
        if (review.BuyerId != GetCurrentUserId() && !User.IsInRole("Admin"))
            return Forbid();

        return await _service.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static ReviewDto ToDto(Review review)
        => new()
        {
            Id = review.Id,
            OrderId = review.OrderId,
            ListingId = review.ListingId,
            BuyerId = review.BuyerId,
            BuyerName = review.Buyer?.Name ?? "",
            Rating = review.Rating,
            Text = review.Text,
            CreatedAt = review.CreatedAt
        };
}

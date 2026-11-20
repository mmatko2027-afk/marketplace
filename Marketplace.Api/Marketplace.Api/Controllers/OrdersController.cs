using System.Security.Claims;
using Marketplace.Application.DTOs;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;

    public OrdersController(OrderService service) => _service = service;
    /// <summary>Забезпечує створення, перегляд та оновлення статусу замовлень користувачів маркетплейсу з перевіркою прав доступу.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        var buyerId = GetCurrentUserId();
        if (buyerId is null)
            return Unauthorized();

        var order = await _service.CreateAsync(buyerId.Value, dto.ListingIds);
        if (order is null)
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Замовлення не створено",
                detail: "Одне або більше оголошень недоступні для замовлення.");

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToDto(order));
    }
    /// <summary>Отримує список замовлень поточного користувача. Доступно лише власнику замовлень.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var buyerId = GetCurrentUserId();
        if (buyerId is null)
            return Unauthorized();

        var orders = await _service.GetByBuyerAsync(buyerId.Value);
        return Ok(orders.Select(ToDto));
    }
    /// <summary>Отримує інформацію про замовлення за його ідентифікатором. Доступно лише власнику замовлення або адміністратору.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _service.GetByIdAsync(id);
        if (order is null)
            return NotFound();

        if (!User.IsInRole("Admin") && order.BuyerId != GetCurrentUserId())
            return Forbid();

        return Ok(ToDto(order));
    }
    /// <summary>Оновлює статус замовлення. Доступно лише адміністратору або покупцю для скасування замовлення.</summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusDto dto)
    {
        var order = await _service.GetByIdAsync(id);
        if (order is null)
            return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var isBuyerCancellation = order.BuyerId == GetCurrentUserId() && dto.Status == "Cancelled";
        if (!isAdmin && !isBuyerCancellation)
            return Forbid();

        var updated = await _service.UpdateStatusAsync(id, dto.Status);
        return updated ? NoContent() : NotFound();
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static OrderDto ToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(item => new OrderItemDto
            {
                ListingId = item.ListingId,
                Title = item.TitleSnapshot,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }
}

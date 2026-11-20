
using Marketplace.Application.Services;
using Marketplace.Application.DTOs;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    /// <summary>Забезпечує створення, перегляд, редагування та видалення повідомлень між користувачами маркетплейсу з перевіркою прав доступу учасників листування.</summary>
    private readonly MessageService _service;

    public MessagesController(MessageService service)
    {
        _service = service;
    }
    /// <summary>Отримує список повідомлень користувача. Доступно лише власнику повідомлень.</summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        if (GetCurrentUserId() != userId)
            return Forbid();

        var messages = await _service.GetByUserAsync(userId);

        return Ok(messages.Select(ToDto));
    }
    /// <summary>Отримує інформацію про повідомлення за його ідентифікатором. Доступно лише учасникам листування.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var message = await _service.GetByIdAsync(id);
        if (message is null)
            return NotFound();

        if (!IsParticipant(message))
            return Forbid();

        return Ok(ToDto(message));
    }
    /// <summary>Створює нове повідомлення. Доступно лише відправнику повідомлення.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateMessageDto dto)
    {
        if (GetCurrentUserId() != dto.SenderId)
            return Forbid();

        var message = await _service.CreateAsync(dto);

        if (message == null)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Повідомлення не створено",
                detail: "Перевірте відправника, отримувача та оголошення.");
        }

        return CreatedAtAction(nameof(GetById), new { id = message.Id }, ToDto(message));
    }
    /// <summary>Оновлює текст повідомлення за його ідентифікатором. Доступно лише відправнику повідомлення.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateMessageDto dto)
    {
        var message = await _service.GetByIdAsync(id);
        if (message is null)
            return NotFound();

        if (GetCurrentUserId() != message.SenderId)
            return Forbid();

        var updated = await _service.UpdateAsync(id, dto.Text.Trim());
        return updated ? NoContent() : NotFound();
    }
    /// <summary>Видаляє повідомлення за його ідентифікатором. Доступно лише відправнику повідомлення.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _service.GetByIdAsync(id);
        if (message is null)
            return NotFound();

        if (GetCurrentUserId() != message.SenderId)
            return Forbid();

        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private bool IsParticipant(Message message)
    {
        var userId = GetCurrentUserId();
        return userId == message.SenderId || userId == message.ReceiverId;
    }

    private static MessageDto ToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            Text = message.Text,
            SentAt = message.SentAt,
            SenderId = message.SenderId,
            SenderName = message.Sender?.Name ?? "",
            ReceiverId = message.ReceiverId,
            ReceiverName = message.Receiver?.Name ?? "",
            ListingId = message.ListingId
        };
    }
}


using Marketplace.Application.Services;
using Marketplace.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly MessageService _service;

    public MessagesController(MessageService service)
    {
        _service = service;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var messages = await _service.GetByUserAsync(userId);

        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMessageDto dto)
    {
        var message = await _service.CreateAsync(dto);

        if (message == null)
        {
            return BadRequest(
                "Перевірте відправника, отримувача та оголошення.");
        }

        return Ok(message);
    }
}

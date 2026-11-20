using Marketplace.Application.DTOs;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/product-conditions")]
public class ProductConditionsController : ControllerBase
{
    private readonly ProductConditionService _service;

    public ProductConditionsController(ProductConditionService service) => _service = service;
    /// <summary>Забезпечує перегляд, створення, редагування та видалення станів товарів у маркетплейсі ClickBack.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var condition = await _service.GetByIdAsync(id);
        return condition is null ? NotFound() : Ok(condition);
    }
    /// <summary>Створює новий стан товару. Доступно лише адміністратору.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var condition = new ProductCondition { Name = dto.Name.Trim() };
        if (!await _service.AddAsync(condition))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Стан товару вже існує");
        return CreatedAtAction(nameof(GetById), new { id = condition.Id }, condition);
    }
    /// <summary>Оновлює назву стану товару. Доступно лише адміністратору.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
        => await _service.UpdateAsync(id, dto.Name.Trim()) ? NoContent() : NotFound();
    /// <summary>Видаляє стан товару за його ідентифікатором. Доступно лише адміністратору.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();
}

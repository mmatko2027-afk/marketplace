using Marketplace.Application.DTOs;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    /// <summary>Забезпечує перегляд, створення, редагування та видалення брендів товарів у маркетплейсі ClickBack.</summary>
    private readonly BrandService _service;

    public BrandsController(BrandService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
    /// <summary>Оновлює назву бренду. Доступно лише адміністратору.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var brand = await _service.GetByIdAsync(id);
        return brand is null ? NotFound() : Ok(brand);
    }
    
    /// <summary>Створює новий бренд. Доступно лише адміністратору.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var brand = new Brand { Name = dto.Name.Trim() };
        if (!await _service.AddAsync(brand))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Бренд уже існує");
        return CreatedAtAction(nameof(GetById), new { id = brand.Id }, brand);
    }

    /// <summary>Оновлює назву бренду. Доступно лише адміністратору.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
        => await _service.UpdateAsync(id, dto.Name.Trim()) ? NoContent() : NotFound();
    /// <summary>Видаляє бренд за його ідентифікатором. Доступно лише адміністратору.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();
}

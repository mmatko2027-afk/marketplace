
using Marketplace.Application.DTOs;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    /// <summary>Забезпечує перегляд, створення, редагування та видалення категорій товарів у маркетплейсі ClickBack.</summary>
    private readonly CategoryService _service;

    public CategoriesController(CategoryService service)
    {
        _service = service;
    }
    /// <summary>Отримує список усіх категорій товарів. Доступно всім користувачам.</summary>

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _service.GetAllAsync();

        return Ok(categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            ListingsCount = c.Listings.Count
        }));
    }
    /// <summary>Отримує інформацію про категорію за її ідентифікатором. Доступно всім користувачам.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            ListingsCount = category.Listings.Count
        });
    }
    /// <summary>Створює нову категорію товарів. Доступно лише адміністратору.</summary>
    [HttpPost]
[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var category = new Marketplace.Domain.Entities.Category
        {
            Name = dto.Name
        };

        await _service.AddAsync(category);

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ListingsCount = 0
            });
    }

    /// <summary>Оновлює назву категорії товарів. Доступно лише адміністратору.</summary>
    [HttpPut("{id}")]
[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        CreateCategoryDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto.Name);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    /// <summary>Видаляє категорію товарів за її ідентифікатором. Доступно лише адміністратору.</summary>
    [HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

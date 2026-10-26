
using Marketplace.Application.DTOs;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/listings")]
public class ListingsController : ControllerBase
{
    private readonly ListingService _service;

    public ListingsController(ListingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 5,
        string? search = null,
        int? categoryId = null,
        string? sort = null)
    {
        var listings = await _service.GetAllAsync(
            page, pageSize, search, categoryId, sort);

        var result = listings.Select(ToDto).ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var listing = await _service.GetByIdAsync(id);

        if (listing == null)
        {
            return NotFound();
        }

        return Ok(ToDto(listing));
    }


    [HttpPost]
    public async Task<IActionResult> Create(CreateListingDto dto)
    {
        var categoryExists =
            await _service.CategoryExistsAsync(dto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest(
                $"Категорію з ID {dto.CategoryId} не знайдено.");
        }

        var listing = new Listing
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            SellerId = dto.SellerId,
            Status = dto.Status
        };

        var created = await _service.CreateAsync(listing);

        if (!created)
        {
            return BadRequest(
                $"Не вдалося створити оголошення. Перевірте SellerId: {dto.SellerId}.");
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = listing.Id },
            ToDto(listing));
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        CreateListingDto dto)
    {
        var existing = await _service.GetByIdAsync(id);

        if (existing == null)
        {
            return NotFound();
        }

        var categoryExists =
            await _service.CategoryExistsAsync(dto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Такої категорії не існує.");
        }

        var updatedListing = new Listing
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            Status = dto.Status
        };

        await _service.UpdateAsync(id, updatedListing);

        return NoContent();
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

    private static ListingDto ToDto(Listing listing)
    {
        return new ListingDto
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            CategoryId = listing.CategoryId,
            CategoryName = listing.Category?.Name ?? "",
            Status = listing.Status

        };
    }
}

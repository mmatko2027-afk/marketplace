
using Marketplace.Application.DTOs;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly ListingService _service;

    public ListingsController(ListingService service)
    {
        _service = service;
    }
    /// <summary>Забезпечує перегляд, створення, редагування та видалення оголошень з перевіркою прав доступу користувачів.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 5,
        string? search = null,
        int? categoryId = null,
        int? brandId = null,
        int? productConditionId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sort = null)
    {
        var listings = await _service.GetAllAsync(page, pageSize, search,
            categoryId, brandId, productConditionId, minPrice, maxPrice, sort);

        var result = new PagedResultDto<ListingDto>
        {
            Items = listings.Items.Select(ToDto).ToList(),
            TotalCount = listings.TotalCount,
            Page = listings.Page,
            PageSize = listings.PageSize
        };

        return Ok(result);
    }
    /// <summary>Отримує інформацію про оголошення за його ідентифікатором. Доступно всім користувачам.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var listing = await _service.GetByIdAsync(id);

        if (listing == null)
            return NotFound();

        return Ok(ToDto(listing));
    }
    
    /// <summary>Створює нове оголошення. Доступно лише зареєстрованим користувачам.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateListingDto dto)
    {
        var sellerId = GetCurrentUserId();
        if (sellerId is null)
            return Unauthorized();

        var categoryExists =
            await _service.CategoryExistsAsync(dto.CategoryId);

        if (!categoryExists)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректна категорія",
                detail: $"Категорію з ID {dto.CategoryId} не знайдено.");

        if (dto.BrandId is not null && !await _service.BrandExistsAsync(dto.BrandId.Value))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний бренд", detail: "Вказаний бренд не знайдено.");

        if (dto.ProductConditionId is not null &&
            !await _service.ProductConditionExistsAsync(dto.ProductConditionId.Value))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний стан", detail: "Вказаний стан товару не знайдено.");

        var listing = new Listing
        {
            Title = dto.Title,
            Description = dto.Description,
            Model = dto.Model,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            SellerId = sellerId.Value,
            BrandId = dto.BrandId,
            ProductConditionId = dto.ProductConditionId,
            ProductionYear = dto.ProductionYear,
            Warranty = dto.Warranty,
            Defects = dto.Defects,
            PackageContents = dto.PackageContents,
            Status = dto.Status,
            Specifications = dto.Specifications.Select(s => new ListingSpecification
            {
                Name = s.Name,
                Value = s.Value
            }).ToList()
        };

        var created = await _service.CreateAsync(listing);

        if (!created)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Оголошення не створено",
                detail: "Не вдалося створити оголошення.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = listing.Id },
            ToDto(listing));
    }
    /// <summary>Оновлює інформацію про оголошення за його ідентифікатором. Доступно лише власнику оголошення або адміністратору.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateListingDto dto)
    {
        var existing = await _service.GetByIdAsync(id);

        if (existing == null)
            return NotFound();

        if (!CanManage(existing.SellerId))
            return Forbid();

        var categoryExists =
            await _service.CategoryExistsAsync(dto.CategoryId);

        if (!categoryExists)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректна категорія", detail: "Такої категорії не існує.");

        if (dto.BrandId is not null && !await _service.BrandExistsAsync(dto.BrandId.Value))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний бренд", detail: "Вказаний бренд не знайдено.");

        if (dto.ProductConditionId is not null &&
            !await _service.ProductConditionExistsAsync(dto.ProductConditionId.Value))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний стан", detail: "Вказаний стан товару не знайдено.");

        var updatedListing = new Listing
        {
            Title = dto.Title,
            Description = dto.Description,
            Model = dto.Model,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            SellerId = existing.SellerId,
            BrandId = dto.BrandId,
            ProductConditionId = dto.ProductConditionId,
            ProductionYear = dto.ProductionYear,
            Warranty = dto.Warranty,
            Defects = dto.Defects,
            PackageContents = dto.PackageContents,
            Specifications = dto.Specifications.Select(s => new ListingSpecification
            {
                Name = s.Name,
                Value = s.Value
            }).ToList(),
            Status = dto.Status
        };

        var updated = await _service.UpdateAsync(id, updatedListing);

        if (!updated)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Оголошення не оновлено", detail: "Не вдалося оновити оголошення.");

        return NoContent();
    }
    /// <summary>Видаляє оголошення за його ідентифікатором. Доступно лише власнику оголошення або адміністратору.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        if (!CanManage(existing.SellerId))
            return Forbid();

        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static ListingDto ToDto(Listing listing)
    {
        return new ListingDto
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Model = listing.Model,
            Price = listing.Price,
            CategoryId = listing.CategoryId,
            CategoryName = listing.Category?.Name ?? "",
            BrandId = listing.BrandId,
            BrandName = listing.Brand?.Name ?? "",
            ProductConditionId = listing.ProductConditionId,
            ProductConditionName = listing.ProductCondition?.Name ?? "",
            ProductionYear = listing.ProductionYear,
            Warranty = listing.Warranty,
            Defects = listing.Defects,
            PackageContents = listing.PackageContents,
            Specifications = listing.Specifications.Select(s => new ListingSpecificationDto
            {
                Name = s.Name,
                Value = s.Value
            }).ToList(),
            Status = listing.Status
        };
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private bool CanManage(int sellerId)
    {
        return User.IsInRole("Admin") || GetCurrentUserId() == sellerId;
    }
}

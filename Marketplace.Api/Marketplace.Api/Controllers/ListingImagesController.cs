using Marketplace.Application.Files;
using Marketplace.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/listings/{listingId:int}/images")]
public sealed class ListingImagesController : ControllerBase
{
    private readonly ListingService _listingService;
    private readonly ListingImageService _imageService;

    public ListingImagesController(ListingService listingService, ListingImageService imageService)
    {
        _listingService = listingService;
        _imageService = imageService;
    }
    /// <summary>Забезпечує керування зображеннями оголошень: перегляд, завантаження, додавання та видалення з перевіркою прав доступу.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(int listingId)
    {
        if (await _listingService.GetByIdAsync(listingId) is null)
            return NotFound();

        var images = await _imageService.GetByListingAsync(listingId);
        return Ok(images.Select(image => new { image.Id, image.ListingId, image.SortOrder }));
    }
    /// <summary>Завантажує зображення оголошення за його ідентифікатором. Доступно всім користувачам.</summary>
    [HttpGet("{imageId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(int listingId, int imageId)
    {
        var image = await _imageService.GetByIdAsync(imageId);
        if (image is null || image.ListingId != listingId)
            return NotFound();

        var path = _imageService.GetAbsolutePath(image.FilePath);
        if (!System.IO.File.Exists(path))
            return NotFound();

        var contentType = GetContentType(Path.GetExtension(path));
        return PhysicalFile(path, contentType, enableRangeProcessing: true);
    }

    /// <summary>Додає зображення до оголошення. Доступно лише власнику оголошення або адміністратору.</summary>
    [HttpPost]
    [Authorize]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Upload(int listingId, IFormFile file, CancellationToken cancellationToken)
    {
        var listing = await _listingService.GetByIdAsync(listingId);
        if (listing is null)
            return NotFound();
        if (!CanManage(listing.SellerId))
            return Forbid();
        if (file is null)
            return BadRequest("Файл є обов'язковим.");

        await using var stream = file.OpenReadStream();
        try
        {
            var image = await _imageService.UploadAsync(
                listingId,
                new ImageUpload(stream, file.Length, Path.GetExtension(file.FileName), file.ContentType),
                cancellationToken);
            return CreatedAtAction(nameof(Download), new { listingId, imageId = image.Id },
                new { image.Id, image.ListingId, image.SortOrder });
        }
        catch (InvalidDataException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректне зображення", detail: exception.Message);
        }
    }
    /// <summary>Видаляє зображення оголошення за його ідентифікатором. Доступно лише власнику оголошення або адміністратору.</summary>
    [HttpDelete("{imageId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int listingId, int imageId, CancellationToken cancellationToken)
    {
        var image = await _imageService.GetByIdAsync(imageId);
        if (image is null || image.ListingId != listingId)
            return NotFound();
        if (!CanManage(image.Listing?.SellerId ?? 0))
            return Forbid();

        await _imageService.DeleteAsync(image, cancellationToken);
        return NoContent();
    }

    private bool CanManage(int sellerId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return User.IsInRole("Admin") || int.TryParse(userId, out var currentUserId) && currentUserId == sellerId;
    }

    private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };
}

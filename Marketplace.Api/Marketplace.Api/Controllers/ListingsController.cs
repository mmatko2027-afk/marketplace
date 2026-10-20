using Marketplace.Application.DTOs;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/listings")]
public class ListingsController : ControllerBase
{
    private readonly MarketplaceDbContext _context;

    public ListingsController(MarketplaceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ListingDto>>> GetListings(
        int page = 1,
        int pageSize = 5,
        string? search = null,
        int? categoryId = null,
        string? sort = null)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 5;
        }

        var query = _context.Listings
            .Include(l => l.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(l =>
                l.Title.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(l =>
                l.CategoryId == categoryId.Value);
        }

        if (sort == "price")
        {
            query = query.OrderBy(l => l.Price);
        }
        else if (sort == "price_desc")
        {
            query = query.OrderByDescending(l => l.Price);
        }
        else
        {
            query = query.OrderBy(l => l.Title);
        }

        var listings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListingDto
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                Price = l.Price,
                CategoryId = l.CategoryId,
                CategoryName = l.Category != null
                    ? l.Category.Name
                    : "",
                Status = l.Status
            })
            .ToListAsync();

        return Ok(listings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ListingDto>> GetListing(int id)
    {
        var listing = await _context.Listings
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listing == null)
        {
            return NotFound();
        }

        var result = new ListingDto
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Price = listing.Price,
            CategoryId = listing.CategoryId,
            CategoryName = listing.Category?.Name ?? "",
            Status = listing.Status
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Listing>> CreateListing(
        CreateListingDto dto)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Такої категорії не існує.");
        }

        var listing = new Listing
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            Status = dto.Status
        };

        _context.Listings.Add(listing);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetListing),
            new { id = listing.Id },
            listing);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateListing(
        int id,
        CreateListingDto dto)
    {
        var listing = await _context.Listings
            .FindAsync(id);

        if (listing == null)
        {
            return NotFound();
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Такої категорії не існує.");
        }

        listing.Title = dto.Title;
        listing.Description = dto.Description;
        listing.Price = dto.Price;
        listing.CategoryId = dto.CategoryId;
        listing.Status = dto.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteListing(int id)
    {
        var listing = await _context.Listings
            .FindAsync(id);

        if (listing == null)
        {
            return NotFound();
        }

        _context.Listings.Remove(listing);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
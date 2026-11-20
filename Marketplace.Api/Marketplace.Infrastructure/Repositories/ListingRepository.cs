using Marketplace.Application.DTOs;
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class ListingRepository : IListingRepository
{
    private readonly MarketplaceDbContext _context;

    public ListingRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<Listing>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        int? categoryId,
        int? brandId,
        int? productConditionId,
        decimal? minPrice,
        decimal? maxPrice,
        string? sort)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 5;
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Seller)
            .Include(l => l.Brand)
            .Include(l => l.ProductCondition)
            .Include(l => l.Specifications)
            .Include(l => l.Images)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(l =>
                l.Title.Contains(search) ||
                l.Description.Contains(search) ||
                l.Model.Contains(search));
        }

        if (brandId.HasValue)
        {
            query = query.Where(l => l.BrandId == brandId.Value);
        }

        if (productConditionId.HasValue)
        {
            query = query.Where(l => l.ProductConditionId == productConditionId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(l => l.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(l => l.Price <= maxPrice.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(l => l.CategoryId == categoryId.Value);
        }

        query = sort?.Trim().ToLowerInvariant() switch
        {
            "price" => query.OrderBy(l => l.Price),
            "price_desc" => query.OrderByDescending(l => l.Price),
            "date" => query.OrderBy(l => l.CreatedAt),
            "date_desc" => query.OrderByDescending(l => l.CreatedAt),
            _ => query.OrderBy(l => l.Title)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<Listing>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Listing?> GetByIdAsync(int id)
    {
        return await _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Seller)
            .Include(l => l.Brand)
            .Include(l => l.ProductCondition)
            .Include(l => l.Specifications)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<bool> AddAsync(Listing listing)
    {
        var categoryExists = await CategoryExistsAsync(listing.CategoryId);
        var brandExists = listing.BrandId is null || await BrandExistsAsync(listing.BrandId.Value);
        var conditionExists = listing.ProductConditionId is null || await ProductConditionExistsAsync(listing.ProductConditionId.Value);
        var sellerExists = await _context.Users
            .AnyAsync(u => u.Id == listing.SellerId);

        if (!categoryExists || !brandExists || !conditionExists || !sellerExists)
        {
            return false;
        }

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(int id, Listing listing)
    {
        var existing = await _context.Listings
            .Include(l => l.Specifications)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (existing == null)
        {
            return false;
        }

        var categoryExists = await CategoryExistsAsync(listing.CategoryId);

        if (!categoryExists)
        {
            return false;
        }

        existing.Title = listing.Title;
        existing.Description = listing.Description;
        existing.Model = listing.Model;
        existing.Price = listing.Price;
        existing.CategoryId = listing.CategoryId;
        existing.BrandId = listing.BrandId;
        existing.ProductConditionId = listing.ProductConditionId;
        existing.ProductionYear = listing.ProductionYear;
        existing.Warranty = listing.Warranty;
        existing.Defects = listing.Defects;
        existing.PackageContents = listing.PackageContents;
        existing.Status = listing.Status;

        _context.ListingSpecifications.RemoveRange(existing.Specifications);
        existing.Specifications = listing.Specifications
            .Select(specification => new ListingSpecification
            {
                ListingId = existing.Id,
                Name = specification.Name,
                Value = specification.Value
            })
            .ToList();

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var listing = await _context.Listings.FindAsync(id);

        if (listing == null)
        {
            return false;
        }

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories
            .AnyAsync(c => c.Id == categoryId);
    }

    public Task<bool> BrandExistsAsync(int brandId)
    {
        return _context.Brands.AnyAsync(b => b.Id == brandId);
    }

    public Task<bool> ProductConditionExistsAsync(int productConditionId)
    {
        return _context.ProductConditions.AnyAsync(c => c.Id == productConditionId);
    }
}
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

    public async Task<List<Listing>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        int? categoryId,
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
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(l =>
                l.Title.Contains(search) ||
                l.Description.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(l => l.CategoryId == categoryId.Value);
        }

        query = sort?.ToLower() switch
        {
            "price" => query.OrderBy(l => l.Price),
            "price_desc" => query.OrderByDescending(l => l.Price),
            _ => query.OrderBy(l => l.Title)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Listing?> GetByIdAsync(int id)
    {
        return await _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Seller)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<bool> AddAsync(Listing listing)
    {
        var categoryExists = await CategoryExistsAsync(listing.CategoryId);
        var sellerExists = await _context.Users
            .AnyAsync(u => u.Id == listing.SellerId);

        if (!categoryExists || !sellerExists)
        {
            return false;
        }

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(int id, Listing listing)
    {
        var existing = await _context.Listings.FindAsync(id);

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
        existing.Price = listing.Price;
        existing.CategoryId = listing.CategoryId;
        existing.Status = listing.Status;

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
}
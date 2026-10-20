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

    public async Task<List<Listing>> GetAllAsync()
    {
        return await _context.Listings
            .Include(x => x.Category)
            .Include(x => x.Seller)
            .ToListAsync();
    }

    public async Task<Listing?> GetByIdAsync(int id)
    {
        return await _context.Listings
            .Include(x => x.Category)
            .Include(x => x.Seller)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Listing listing)
    {
        await _context.Listings.AddAsync(listing);
        await _context.SaveChangesAsync();
    }
}
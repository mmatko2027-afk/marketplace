
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class FavoritesRepository : IFavoritesRepository
{
    private readonly MarketplaceDbContext _context;

    public FavoritesRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public async Task<List<Favorite>> GetByUserAsync(int userId)
    {
        return await _context.Favorites
            .Include(f => f.Listing)
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> AddAsync(int userId, int listingId)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == userId);

        var listingExists = await _context.Listings
            .AnyAsync(l => l.Id == listingId);

        if (!userExists || !listingExists)
        {
            return false;
        }

        var alreadyExists = await _context.Favorites
            .AnyAsync(f =>
                f.UserId == userId &&
                f.ListingId == listingId);

        if (alreadyExists)
        {
            return false;
        }

        var favorite = new Favorite
        {
            UserId = userId,
            ListingId = listingId
        };

        _context.Favorites.Add(favorite);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var favorite = await _context.Favorites
            .FindAsync(id);

        if (favorite == null)
        {
            return false;
        }

        _context.Favorites.Remove(favorite);

        await _context.SaveChangesAsync();

        return true;
    }
}

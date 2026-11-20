using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class ListingImageRepository : IListingImageRepository
{
    private readonly MarketplaceDbContext _context;

    public ListingImageRepository(MarketplaceDbContext context) => _context = context;

    public Task<ListingImage?> GetByIdAsync(int id)
        => _context.ListingImages.Include(i => i.Listing).AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);

    public Task<List<ListingImage>> GetByListingAsync(int listingId)
        => _context.ListingImages.AsNoTracking().Where(i => i.ListingId == listingId).OrderBy(i => i.SortOrder).ToListAsync();

    public async Task<ListingImage> AddAsync(ListingImage image)
    {
        _context.ListingImages.Add(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var image = await _context.ListingImages.FindAsync(id);
        if (image is null) return false;
        _context.ListingImages.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }
}

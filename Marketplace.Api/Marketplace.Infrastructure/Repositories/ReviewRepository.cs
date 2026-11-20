using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly MarketplaceDbContext _context;

    public ReviewRepository(MarketplaceDbContext context) => _context = context;

    public async Task<Review?> CreateAsync(int buyerId, int orderId, int listingId, int rating, string text)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == buyerId);

        if (order is null || order.Status != "Delivered" ||
            !order.Items.Any(item => item.ListingId == listingId) ||
            await _context.Reviews.AnyAsync(r => r.OrderId == orderId && r.ListingId == listingId))
            return null;

        var review = new Review
        {
            BuyerId = buyerId,
            OrderId = orderId,
            ListingId = listingId,
            Rating = rating,
            Text = text.Trim()
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(review.Id);
    }

    public Task<Review?> GetByIdAsync(int id)
        => Query().FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<Review>> GetByListingAsync(int listingId)
        => Query().Where(r => r.ListingId == listingId).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<bool> UpdateAsync(int id, int rating, string text)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review is null) return false;
        review.Rating = rating;
        review.Text = text.Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review is null) return false;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<Review> Query()
        => _context.Reviews.Include(r => r.Buyer).AsNoTracking();
}

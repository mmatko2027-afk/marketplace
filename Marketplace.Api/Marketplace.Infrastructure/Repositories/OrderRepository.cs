using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly MarketplaceDbContext _context;

    public OrderRepository(MarketplaceDbContext context) => _context = context;

    public async Task<Order?> CreateAsync(int buyerId, IReadOnlyCollection<int> listingIds)
    {
        var ids = listingIds.Distinct().ToArray();
        if (ids.Length == 0 || !await _context.Users.AnyAsync(u => u.Id == buyerId))
            return null;

        var listings = await _context.Listings
            .Where(l => ids.Contains(l.Id) && l.Status == "active")
            .ToListAsync();

        if (listings.Count != ids.Length)
            return null;

        var order = new Order
        {
            BuyerId = buyerId,
            Status = "Pending",
            Items = listings.Select(listing => new OrderItem
            {
                ListingId = listing.Id,
                UnitPrice = listing.Price,
                TitleSnapshot = listing.Title
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(item => item.UnitPrice);
        foreach (var listing in listings)
            listing.Status = "reserved";

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(order.Id);
    }

    public Task<List<Order>> GetByBuyerAsync(int buyerId)
    {
        return Query()
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public Task<Order?> GetByIdAsync(int id)
        => Query().FirstOrDefaultAsync(o => o.Id == id);

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
            return false;

        order.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<Order> Query()
    {
        return _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Listing)
            .AsNoTracking();
    }
}

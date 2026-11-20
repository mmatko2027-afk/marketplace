
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class MessagesRepository : IMessagesRepository
{
    private readonly MarketplaceDbContext _context;

    public MessagesRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public async Task<List<Message>> GetByUserAsync(int userId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Listing)
            .AsNoTracking()
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public Task<Message?> GetByIdAsync(int id)
    {
        return _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Listing)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Message?> CreateAsync(Message message)
    {
        var senderExists = await _context.Users
            .AnyAsync(u => u.Id == message.SenderId);

        var receiverExists = await _context.Users
            .AnyAsync(u => u.Id == message.ReceiverId);

        var listingExists = await _context.Listings
            .AnyAsync(l => l.Id == message.ListingId);

        if (!senderExists || !receiverExists || !listingExists)
        {
            return null;
        }

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Listing)
            .FirstOrDefaultAsync(m => m.Id == message.Id);
    }

    public async Task<bool> UpdateAsync(int id, string text)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message is null)
            return false;

        message.Text = text;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message is null)
            return false;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }
}


using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly MarketplaceDbContext _context;

    public UsersRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> CreateAsync(User user)
    {
        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email == user.Email);

        if (emailExists)
            return null;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }
}

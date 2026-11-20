
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
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
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

    public async Task<bool> UpdateAsync(int id, string name, string email, string role)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return false;

        var emailInUse = await _context.Users
            .AnyAsync(u => u.Id != id && u.Email == email);

        if (emailInUse)
            return false;

        user.Name = name;
        user.Email = email;
        user.Role = role;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}

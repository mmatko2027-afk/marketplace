using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly MarketplaceDbContext _context;

    public BrandRepository(MarketplaceDbContext context) => _context = context;

    public Task<List<Brand>> GetAllAsync() => _context.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync();

    public Task<Brand?> GetByIdAsync(int id) => _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public async Task<bool> AddAsync(Brand brand)
    {
        if (await _context.Brands.AnyAsync(b => b.Name == brand.Name)) return false;
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(int id, string name)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand is null) return false;
        brand.Name = name;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand is null) return false;
        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
        return true;
    }
}

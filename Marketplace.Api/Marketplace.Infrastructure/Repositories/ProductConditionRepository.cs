using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class ProductConditionRepository : IProductConditionRepository
{
    private readonly MarketplaceDbContext _context;

    public ProductConditionRepository(MarketplaceDbContext context) => _context = context;

    public Task<List<ProductCondition>> GetAllAsync() => _context.ProductConditions.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

    public Task<ProductCondition?> GetByIdAsync(int id) => _context.ProductConditions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<bool> AddAsync(ProductCondition condition)
    {
        if (await _context.ProductConditions.AnyAsync(c => c.Name == condition.Name)) return false;
        _context.ProductConditions.Add(condition);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(int id, string name)
    {
        var condition = await _context.ProductConditions.FindAsync(id);
        if (condition is null) return false;
        condition.Name = name;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var condition = await _context.ProductConditions.FindAsync(id);
        if (condition is null) return false;
        _context.ProductConditions.Remove(condition);
        await _context.SaveChangesAsync();
        return true;
    }
}

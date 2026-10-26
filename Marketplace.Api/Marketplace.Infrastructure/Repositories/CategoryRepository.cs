using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
	private readonly MarketplaceDbContext _context;

	public CategoryRepository(MarketplaceDbContext context)
	{
		_context = context;
	}

	public async Task<List<Category>> GetAllAsync()
	{
		return await _context.Categories
			.Include(c => c.Listings)
			.AsNoTracking()
			.ToListAsync();
	}

	public async Task<Category?> GetByIdAsync(int id)
	{
		return await _context.Categories
			.Include(c => c.Listings)
			.FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task AddAsync(Category category)
	{
		_context.Categories.Add(category);
		await _context.SaveChangesAsync();
	}

	public async Task<bool> UpdateAsync(int id, string name)
	{
		var category = await _context.Categories.FindAsync(id);

		if (category == null)
		{
			return false;
		}

		category.Name = name;
		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var category = await _context.Categories
			.Include(c => c.Listings)
			.FirstOrDefaultAsync(c => c.Id == id);

		if (category == null || category.Listings.Count > 0)
		{
			return false;
		}

		_context.Categories.Remove(category);
		await _context.SaveChangesAsync();

		return true;
	}
}
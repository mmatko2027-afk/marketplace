using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task AddAsync(Category category);

    Task<bool> UpdateAsync(int id, string name);

    Task<bool> DeleteAsync(int id);
}
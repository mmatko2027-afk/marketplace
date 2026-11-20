using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IProductConditionRepository
{
    Task<List<ProductCondition>> GetAllAsync();
    Task<ProductCondition?> GetByIdAsync(int id);
    Task<bool> AddAsync(ProductCondition condition);
    Task<bool> UpdateAsync(int id, string name);
    Task<bool> DeleteAsync(int id);
}

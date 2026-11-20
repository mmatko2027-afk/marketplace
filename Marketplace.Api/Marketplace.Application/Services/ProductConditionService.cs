using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class ProductConditionService
{
    private readonly IProductConditionRepository _repository;

    public ProductConditionService(IProductConditionRepository repository) => _repository = repository;

    public Task<List<ProductCondition>> GetAllAsync() => _repository.GetAllAsync();
    public Task<ProductCondition?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<bool> AddAsync(ProductCondition condition) => _repository.AddAsync(condition);
    public Task<bool> UpdateAsync(int id, string name) => _repository.UpdateAsync(id, name);
    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}

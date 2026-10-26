using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class CategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Category>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task AddAsync(Category category)
    {
        return _repository.AddAsync(category);
    }

    public Task<bool> UpdateAsync(int id, string name)
    {
        return _repository.UpdateAsync(id, name);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }
}
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class BrandService
{
    private readonly IBrandRepository _repository;

    public BrandService(IBrandRepository repository) => _repository = repository;

    public Task<List<Brand>> GetAllAsync() => _repository.GetAllAsync();
    public Task<Brand?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<bool> AddAsync(Brand brand) => _repository.AddAsync(brand);
    public Task<bool> UpdateAsync(int id, string name) => _repository.UpdateAsync(id, name);
    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}

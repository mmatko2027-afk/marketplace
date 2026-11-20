using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IBrandRepository
{
    Task<List<Brand>> GetAllAsync();
    Task<Brand?> GetByIdAsync(int id);
    Task<bool> AddAsync(Brand brand);
    Task<bool> UpdateAsync(int id, string name);
    Task<bool> DeleteAsync(int id);
}

using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IListingRepository
{
    Task<List<Listing>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        int? categoryId,
        string? sort);

    Task<Listing?> GetByIdAsync(int id);

    Task<bool> AddAsync(Listing listing);

    Task<bool> UpdateAsync(int id, Listing listing);

    Task<bool> DeleteAsync(int id);

    Task<bool> CategoryExistsAsync(int categoryId);
}
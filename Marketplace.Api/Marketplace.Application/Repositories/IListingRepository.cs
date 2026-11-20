using Marketplace.Application.DTOs;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IListingRepository
{
    Task<PagedResultDto<Listing>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        int? categoryId,
        int? brandId,
        int? productConditionId,
        decimal? minPrice,
        decimal? maxPrice,
        string? sort);

    Task<Listing?> GetByIdAsync(int id);

    Task<bool> AddAsync(Listing listing);

    Task<bool> UpdateAsync(int id, Listing listing);

    Task<bool> DeleteAsync(int id);

    Task<bool> CategoryExistsAsync(int categoryId);

    Task<bool> BrandExistsAsync(int brandId);

    Task<bool> ProductConditionExistsAsync(int productConditionId);
}
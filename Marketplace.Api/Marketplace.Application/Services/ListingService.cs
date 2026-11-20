using Marketplace.Application.DTOs;
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class ListingService
{
    private readonly IListingRepository _repository;

    public ListingService(IListingRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResultDto<Listing>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        int? categoryId,
        int? brandId,
        int? productConditionId,
        decimal? minPrice,
        decimal? maxPrice,
        string? sort)
    {
        return _repository.GetAllAsync(
            page, pageSize, search, categoryId, brandId,
            productConditionId, minPrice, maxPrice, sort);
    }

    public Task<Listing?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<bool> CreateAsync(Listing listing)
    {
        return _repository.AddAsync(listing);
    }

    public Task<bool> UpdateAsync(int id, Listing listing)
    {
        return _repository.UpdateAsync(id, listing);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<bool> CategoryExistsAsync(int categoryId)
    {
        return _repository.CategoryExistsAsync(categoryId);
    }

    public Task<bool> BrandExistsAsync(int brandId)
    {
        return _repository.BrandExistsAsync(brandId);
    }

    public Task<bool> ProductConditionExistsAsync(int productConditionId)
    {
        return _repository.ProductConditionExistsAsync(productConditionId);
    }
}
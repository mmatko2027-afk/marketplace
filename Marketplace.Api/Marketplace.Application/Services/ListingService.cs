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

    public Task<List<Listing>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        int? categoryId,
        string? sort)
    {
        return _repository.GetAllAsync(
            page, pageSize, search, categoryId, sort);
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
}
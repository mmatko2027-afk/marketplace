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

    public async Task<List<Listing>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Listing?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddAsync(Listing listing)
    {
        await _repository.AddAsync(listing);
    }
}
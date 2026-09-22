using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IListingRepository
{
    Task<List<Listing>> GetAllAsync();

    Task<Listing?> GetByIdAsync(int id);

    Task AddAsync(Listing listing);
}
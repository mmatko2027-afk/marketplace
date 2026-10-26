
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IFavoritesRepository
{
    Task<List<Favorite>> GetByUserAsync(int userId);

    Task<bool> AddAsync(int userId, int listingId);

    Task<bool> DeleteAsync(int id);
}

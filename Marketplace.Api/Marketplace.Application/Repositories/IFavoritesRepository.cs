
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IFavoritesRepository
{
    Task<List<Favorite>> GetByUserAsync(int userId);

    Task<Favorite?> GetByIdAsync(int id);

    Task<bool> AddAsync(int userId, int listingId);

    Task<bool> DeleteAsync(int id);
}

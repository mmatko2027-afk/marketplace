
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class FavoriteService
{
    private readonly IFavoritesRepository _repository;

    public FavoriteService(IFavoritesRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Favorite>> GetByUserAsync(int userId)
    {
        return await _repository.GetByUserAsync(userId);
    }

    public async Task<bool> AddAsync(int userId, int listingId)
    {
        return await _repository.AddAsync(userId, listingId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}

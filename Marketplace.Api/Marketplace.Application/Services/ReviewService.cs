using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class ReviewService
{
    private readonly IReviewRepository _repository;

    public ReviewService(IReviewRepository repository) => _repository = repository;

    public Task<Review?> CreateAsync(int buyerId, int orderId, int listingId, int rating, string text)
        => _repository.CreateAsync(buyerId, orderId, listingId, rating, text);

    public Task<Review?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<List<Review>> GetByListingAsync(int listingId) => _repository.GetByListingAsync(listingId);
    public Task<bool> UpdateAsync(int id, int rating, string text) => _repository.UpdateAsync(id, rating, text);
    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}

using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IReviewRepository
{
    Task<Review?> CreateAsync(int buyerId, int orderId, int listingId, int rating, string text);
    Task<Review?> GetByIdAsync(int id);
    Task<List<Review>> GetByListingAsync(int listingId);
    Task<bool> UpdateAsync(int id, int rating, string text);
    Task<bool> DeleteAsync(int id);
}

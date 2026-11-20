using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IListingImageRepository
{
    Task<ListingImage?> GetByIdAsync(int id);
    Task<List<ListingImage>> GetByListingAsync(int listingId);
    Task<ListingImage> AddAsync(ListingImage image);
    Task<bool> DeleteAsync(int id);
}

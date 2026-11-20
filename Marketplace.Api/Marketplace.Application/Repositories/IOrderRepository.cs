using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IOrderRepository
{
    Task<Order?> CreateAsync(int buyerId, IReadOnlyCollection<int> listingIds);
    Task<List<Order>> GetByBuyerAsync(int buyerId);
    Task<Order?> GetByIdAsync(int id);
    Task<bool> UpdateStatusAsync(int id, string status);
}

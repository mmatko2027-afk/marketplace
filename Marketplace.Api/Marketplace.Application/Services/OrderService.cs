using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository) => _repository = repository;

    public Task<Order?> CreateAsync(int buyerId, IReadOnlyCollection<int> listingIds)
        => _repository.CreateAsync(buyerId, listingIds);

    public Task<List<Order>> GetByBuyerAsync(int buyerId)
        => _repository.GetByBuyerAsync(buyerId);

    public Task<Order?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public Task<bool> UpdateStatusAsync(int id, string status)
        => _repository.UpdateStatusAsync(id, status);
}

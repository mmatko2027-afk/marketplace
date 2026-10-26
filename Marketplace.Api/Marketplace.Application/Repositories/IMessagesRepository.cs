
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IMessagesRepository
{
    Task<List<Message>> GetByUserAsync(int userId);

    Task<Message?> CreateAsync(Message message);
}

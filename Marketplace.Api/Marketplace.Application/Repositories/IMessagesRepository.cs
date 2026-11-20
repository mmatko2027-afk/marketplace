
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IMessagesRepository
{
    Task<List<Message>> GetByUserAsync(int userId);

    Task<Message?> GetByIdAsync(int id);

    Task<Message?> CreateAsync(Message message);

    Task<bool> UpdateAsync(int id, string text);

    Task<bool> DeleteAsync(int id);
}

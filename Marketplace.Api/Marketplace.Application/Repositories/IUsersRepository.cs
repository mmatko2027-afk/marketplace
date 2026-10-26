
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IUsersRepository
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<User?> CreateAsync(User user);
}

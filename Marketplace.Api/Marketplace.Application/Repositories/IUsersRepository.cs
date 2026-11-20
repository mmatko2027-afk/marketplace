using Marketplace.Domain.Entities;

namespace Marketplace.Application.Repositories;

public interface IUsersRepository
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByEmailAsync(string email);

    Task<User?> CreateAsync(User user);

    Task<bool> UpdateAsync(int id, string name, string email, string role);

    Task<bool> DeleteAsync(int id);
}
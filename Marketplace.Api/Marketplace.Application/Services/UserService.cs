
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class UserService
{
    private readonly IUsersRepository _repository;

    public UserService(IUsersRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<User?> CreateAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        return await _repository.CreateAsync(user);
    }

    public Task<bool> UpdateAsync(int id, string name, string email, string role)
    {
        return _repository.UpdateAsync(id, name, email, role);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }
}

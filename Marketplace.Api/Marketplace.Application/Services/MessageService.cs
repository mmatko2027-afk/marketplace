
using Marketplace.Application.DTOs;
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class MessageService
{
    private readonly IMessagesRepository _repository;

    public MessageService(IMessagesRepository repository)
    {
        _repository = repository;
    }

    public Task<Message?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<bool> UpdateAsync(int id, string text) => _repository.UpdateAsync(id, text);

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    public async Task<List<Message>> GetByUserAsync(int userId)
    {
        return await _repository.GetByUserAsync(userId);
    }

    public async Task<Message?> CreateAsync(CreateMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            return null;
        }

        var message = new Message
        {
            Text = dto.Text,
            SenderId = dto.SenderId,
            ReceiverId = dto.ReceiverId,
            ListingId = dto.ListingId
        };

        return await _repository.CreateAsync(message);
    }
}

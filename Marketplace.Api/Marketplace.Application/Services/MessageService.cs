
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

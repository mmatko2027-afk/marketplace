namespace Marketplace.Application.Background;

public interface IBackgroundTaskQueue
{
    /// <summary>Додає завдання до черги для асинхронного виконання.</summary>
    ValueTask QueueAsync(Func<CancellationToken, Task> workItem, CancellationToken cancellationToken = default);
    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}

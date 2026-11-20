using System.Threading.Channels;
using Marketplace.Application.Background;

namespace Marketplace.Infrastructure.Background;
/// <summary>Реалізація черги завдань для асинхронного виконання у фоновому режимі.</summary>
public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundTaskQueue(int capacity = 100)
    {
        _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(
            new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.Wait });
    }

    public ValueTask QueueAsync(Func<CancellationToken, Task> workItem, CancellationToken cancellationToken = default) =>
        _queue.Writer.WriteAsync(workItem, cancellationToken);

    public ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken) =>
        _queue.Reader.ReadAsync(cancellationToken);
}

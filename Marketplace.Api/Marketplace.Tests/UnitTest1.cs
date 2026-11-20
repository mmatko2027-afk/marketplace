using Marketplace.Application.Background;
using Marketplace.Infrastructure.Background;

namespace Marketplace.Tests;

public class UnitTest1
{
    [Fact]
    public async Task BackgroundTaskQueue_PreservesQueuedDelegate()
    {
        var queue = new BackgroundTaskQueue();
        Func<CancellationToken, Task> workItem = _ => Task.CompletedTask;

        await queue.QueueAsync(workItem);
        var dequeued = await queue.DequeueAsync(CancellationToken.None);

        Assert.Same(workItem, dequeued);
    }

    [Fact]
    public void InMemoryJobStatusStore_TracksLifecycleAndReturnsNullForUnknownId()
    {
        var store = new InMemoryJobStatusStore();

        var jobId = store.CreateJob();
        Assert.Equal(BackgroundJobStatus.Pending, store.GetJob(jobId)?.Status);

        store.SetResult(jobId, "ok");
        var doneJob = store.GetJob(jobId);
        Assert.Equal(BackgroundJobStatus.Done, doneJob?.Status);
        Assert.Equal("ok", doneJob?.Result);

        Assert.Null(store.GetJob(Guid.NewGuid()));
    }
}
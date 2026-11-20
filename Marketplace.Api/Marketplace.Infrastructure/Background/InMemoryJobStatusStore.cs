using System.Collections.Concurrent;
using Marketplace.Application.Background;

namespace Marketplace.Infrastructure.Background;

public sealed class InMemoryJobStatusStore : IJobStatusStore
{
    private readonly ConcurrentDictionary<Guid, BackgroundJobEntry> _jobs = [];

    public Guid CreateJob()
    {
        var jobId = Guid.NewGuid();
        _jobs[jobId] = new BackgroundJobEntry(BackgroundJobStatus.Pending);
        return jobId;
    }

    public BackgroundJobEntry? GetJob(Guid jobId) =>
        _jobs.TryGetValue(jobId, out var entry) ? entry : null;

    public void SetStatus(Guid jobId, BackgroundJobStatus status)
    {
        _jobs[jobId] = new BackgroundJobEntry(status);
    }

    public void SetResult(Guid jobId, object? result)
    {
        _jobs[jobId] = new BackgroundJobEntry(BackgroundJobStatus.Done, Result: result);
    }

    public void SetError(Guid jobId, string error, BackgroundJobStatus status = BackgroundJobStatus.Failed)
    {
        _jobs[jobId] = new BackgroundJobEntry(status, Error: error);
    }
}
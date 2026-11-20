namespace Marketplace.Application.Background;

public sealed record BackgroundJobEntry(
    BackgroundJobStatus Status,
    object? Result = null,
    string? Error = null);

public sealed record QueuedJobResponse(Guid JobId);

public sealed record JobStatusDto(
    Guid JobId,
    string Status,
    object? Result = null,
    string? Error = null);

public interface IJobStatusStore
{
    Guid CreateJob();
    BackgroundJobEntry? GetJob(Guid jobId);
    void SetStatus(Guid jobId, BackgroundJobStatus status);
    void SetResult(Guid jobId, object? result);
    void SetError(Guid jobId, string error, BackgroundJobStatus status = BackgroundJobStatus.Failed);
}
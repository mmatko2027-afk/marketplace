namespace Marketplace.Application.Background;

public enum BackgroundJobStatus
{
    Pending,
    Processing,
    Done,
    Failed,
    Cancelled
}
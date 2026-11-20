namespace Marketplace.Application.AI;

public interface IAiSupportService
{
    Task<AiSupportResponse> AskAsync(
        AiSupportRequest request,
        CancellationToken cancellationToken = default);
}

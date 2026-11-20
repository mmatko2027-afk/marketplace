namespace Marketplace.Application.Integrations;

public interface IExternalCatalogClient
{
    Task<string?> GetStatusAsync(CancellationToken cancellationToken = default);
}

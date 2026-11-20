using System.Net;
using Marketplace.Application.Integrations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.Integrations;

public sealed class ExternalCatalogClient(
    HttpClient httpClient,
    IMemoryCache cache,
    ILogger<ExternalCatalogClient> logger) : IExternalCatalogClient
{
    private const string CacheKey = "external-catalog-status";

    public async Task<string?> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out string? cached))
            return cached;

        try
        {
            using var response = await httpClient.GetAsync("status", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Зовнішній сервіс повернув HTTP {StatusCode}.", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            cache.Set(CacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Помилка мережевого виклику зовнішнього сервісу.");
            return null;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Тайм-аут зовнішнього сервісу.");
            return null;
        }
    }
}

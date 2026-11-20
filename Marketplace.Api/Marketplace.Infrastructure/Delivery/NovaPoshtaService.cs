using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Marketplace.Application.Delivery;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace Marketplace.Infrastructure.Delivery;

public sealed class NovaPoshtaService(
    HttpClient httpClient,
    IOptions<NovaPoshtaOptions> options,
    IMemoryCache cache,
    ILogger<NovaPoshtaService> logger) : INovaPoshtaService
{
    public async Task<IReadOnlyList<NovaPoshtaCity>> SearchCitiesAsync(
        string query,
        int limit = 10,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Вкажіть назву міста.");
        var settings = options.Value;
        if (limit is < 1 or > 100)
            throw new ArgumentException("Кількість міст має бути від 1 до 100.");
        if (page < 1)
            throw new ArgumentException("Номер сторінки має бути не менше 1.");

        var cacheKey = $"nova-poshta-cities:{query.Trim().ToUpperInvariant()}:{limit}:{page}";
        var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                Math.Max(1, settings.CitySearchCacheMinutes));
            var response = await PostAsync("Address", "searchSettlements", new
            {
                CityName = query.Trim(),
                    Limit = Math.Min(limit, Math.Max(1, settings.CitySearchLimit)).ToString(),
                Page = page.ToString()
            }, cancellationToken);

            return response.Data?.SelectMany(item => item.Addresses ?? [])
                .Select(city => new NovaPoshtaCity(
                    city.Ref ?? "",
                    city.DeliveryCity ?? "",
                    city.Present ?? city.MainDescription ?? ""))
                .Where(city => !string.IsNullOrWhiteSpace(city.DeliveryCity))
                .DistinctBy(city => city.DeliveryCity)
                .ToList() ?? [];
        });

        return result ?? [];
    }

    public async Task<IReadOnlyList<NovaPoshtaPoint>> SearchWarehousesAsync(
        NovaPoshtaSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("Вкажіть назву міста або номер відділення.");

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new InvalidOperationException("Nova Poshta API key не налаштовано.");

        var cityRef = request.CityRef;
        var warehouseQuery = request.Query;
        if (string.IsNullOrWhiteSpace(cityRef))
        {
            var cacheKey = $"nova-poshta-city:{request.Query.Trim().ToUpperInvariant()}";
            cityRef = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                    Math.Max(1, settings.CityCacheMinutes));
                var cityResponse = await PostAsync("Address", "searchSettlements", new
                {
                    CityName = request.Query.Trim(),
                    Limit = Math.Max(1, settings.CitySearchLimit).ToString(),
                    Page = "1"
                }, cancellationToken);
                return cityResponse.Data?.SelectMany(item => item.Addresses ?? [])
                    .Select(city => city.DeliveryCity)
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            });

            if (string.IsNullOrWhiteSpace(cityRef))
                return [];

            warehouseQuery = null;
        }

        var response = await PostAsync("Address", "getWarehouses", new
        {
            CityRef = cityRef,
            FindByString = warehouseQuery,
            Limit = Math.Max(1, settings.WarehouseSearchLimit).ToString(),
            Page = "1"
        }, cancellationToken);

        return response.Data?.Select(point => new NovaPoshtaPoint(
            point.Ref ?? "", point.Description ?? "", point.ShortAddress ?? "")).ToList()
            ?? [];
    }

    private async Task<NovaPoshtaResponse> PostAsync(
        string modelName,
        string calledMethod,
        object methodProperties,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var methodPropertiesJson = System.Text.Json.JsonSerializer.SerializeToElement(
            methodProperties,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null });
        var payload = new
        {
            apiKey = settings.ApiKey,
            modelName,
            calledMethod,
            methodProperties = methodPropertiesJson
        };

        try
        {
            NovaPoshtaResponse? result = null;
            for (var attempt = 0; attempt <= Math.Max(0, settings.RetryCount); attempt++)
            {
                using var response = await httpClient.PostAsJsonAsync("", payload, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<NovaPoshtaResponse>(body);
                    break;
                }

                logger.LogWarning("Nova Poshta returned HTTP {StatusCode}: {Body}",
                    (int)response.StatusCode, body);
                if ((int)response.StatusCode is not (408 or 429 or >= 500) ||
                    attempt >= Math.Max(0, settings.RetryCount))
                    response.EnsureSuccessStatusCode();

                await Task.Delay(TimeSpan.FromMilliseconds(300 * (attempt + 1)), cancellationToken);
            }

            if (result is null)
                throw new InvalidOperationException("Nova Poshta повернула порожню відповідь.");

            if (!result.Success)
            {
                var details = string.Join("; ", result.Errors ?? []);
                if (string.IsNullOrWhiteSpace(details))
                    details = "Зовнішній сервіс Нової пошти повернув success=false без опису помилки.";
                throw new InvalidOperationException($"Nova Poshta: {details}");
            }

            return result;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Nova Poshta request failed.");
            throw new InvalidOperationException("Сервіс Нової пошти тимчасово недоступний.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Nova Poshta request timed out.");
            throw new TimeoutException("Сервіс Нової пошти не відповів вчасно.", exception);
        }
    }

    private sealed class NovaPoshtaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<NovaPoshtaResponseData>? Data { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }
    }

    private sealed class NovaPoshtaResponseData
    {
        [JsonPropertyName("Ref")] public string? Ref { get; set; }
        [JsonPropertyName("DeliveryCity")] public string? DeliveryCity { get; set; }
        [JsonPropertyName("Present")] public string? Present { get; set; }
        [JsonPropertyName("MainDescription")] public string? MainDescription { get; set; }
        [JsonPropertyName("Addresses")] public List<NovaPoshtaCityAddress>? Addresses { get; set; }
        [JsonPropertyName("Description")] public string? Description { get; set; }
        [JsonPropertyName("ShortAddress")] public string? ShortAddress { get; set; }
    }

    private sealed class NovaPoshtaCityAddress
    {
        [JsonPropertyName("Ref")] public string? Ref { get; set; }
        [JsonPropertyName("DeliveryCity")] public string? DeliveryCity { get; set; }
        [JsonPropertyName("Present")] public string? Present { get; set; }
        [JsonPropertyName("MainDescription")] public string? MainDescription { get; set; }
    }

    private sealed class NovaPoshtaWarehouse
    {
        [JsonPropertyName("Ref")] public string? Ref { get; set; }
        [JsonPropertyName("Description")] public string? Description { get; set; }
        [JsonPropertyName("ShortAddress")] public string? ShortAddress { get; set; }
    }
}

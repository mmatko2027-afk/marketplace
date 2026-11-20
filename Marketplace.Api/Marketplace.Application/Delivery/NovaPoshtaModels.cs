namespace Marketplace.Application.Delivery;

public sealed record NovaPoshtaSearchRequest(string Query, string? CityRef = null);
public sealed record NovaPoshtaPoint(string Ref, string Description, string Address);
public sealed record NovaPoshtaCity(string Ref, string DeliveryCity, string Description);

public interface INovaPoshtaService
{
    Task<IReadOnlyList<NovaPoshtaCity>> SearchCitiesAsync(
        string query,
        int limit = 10,
        int page = 1,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NovaPoshtaPoint>> SearchWarehousesAsync(
        NovaPoshtaSearchRequest request,
        CancellationToken cancellationToken = default);
}

namespace Marketplace.Infrastructure.Delivery;

public sealed class NovaPoshtaOptions
{
    public string BaseUrl { get; set; } = "https://api.novaposhta.ua/v2.0/json/";
    public string ApiKey { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 2;
    public int CityCacheMinutes { get; set; } = 60;
    public int CitySearchCacheMinutes { get; set; } = 10;
    public int CitySearchLimit { get; set; } = 10;
    public int WarehouseSearchLimit { get; set; } = 50;
}

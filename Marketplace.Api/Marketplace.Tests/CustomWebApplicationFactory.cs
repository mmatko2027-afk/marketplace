using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DbConnection _connection = new SqliteConnection("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "integration-test-secret-key-32-characters");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "ClickBack.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "ClickBack.Tests.Client");
        Environment.SetEnvironmentVariable("Ai__ApiKey", "");
        Environment.SetEnvironmentVariable("Ai__BaseUrl", "");
        Environment.SetEnvironmentVariable("Ai__Model", "");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "integration-test-secret-key-32-characters",
                ["Jwt:Issuer"] = "ClickBack.Tests",
                ["Jwt:Audience"] = "ClickBack.Tests.Client",
                ["Ai:ApiKey"] = "",
                ["Ai:BaseUrl"] = "",
                ["Ai:Model"] = ""
            });
        });

        builder.ConfigureServices(services =>
        {
            _connection.Open();

            var descriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(DbContextOptions<MarketplaceDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<MarketplaceDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}

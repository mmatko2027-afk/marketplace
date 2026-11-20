using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Marketplace.Application.DTOs.Auth;
using Marketplace.Application.DTOs;
using Marketplace.Domain.Entities;
using Marketplace.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Tests;

public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
   
    public async Task AiSupport_ReturnsServiceUnavailableWhenNotConfigured()
    {
        using var client = _factory.CreateClient();
        var user = await RegisterAsync(client, "AiUser");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

        var response = await client.PostAsJsonAsync("/api/support/ai/ask", new { Question = "Як працює доставка?" });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Register_CreatesUserInTestDatabase()
    {
        using var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = "Integration User",
            Email = email,
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var user = db.Users.SingleOrDefault(u => u.Email == email);

        Assert.NotNull(user);
        Assert.Equal("Integration User", user.Name);
    }

    [Fact]
    public async Task User_CanReadOwnProfile_ButMessageCanOnlyBeChangedBySender()
    {
        using var client = _factory.CreateClient();
        var sender = await RegisterAsync(client, "Sender");
        var receiver = await RegisterAsync(client, "Receiver");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", sender.Token);

        var profileResponse = await client.GetAsync($"/api/users/{sender.Id}");
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
            var category = new Category { Name = $"Category-{Guid.NewGuid():N}" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            db.Listings.Add(new Listing
            {
                Title = "Test listing",
                CategoryId = category.Id,
                SellerId = sender.Id,
                Price = 100
            });
            await db.SaveChangesAsync();
        }

        int listingId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
            listingId = db.Listings.OrderByDescending(l => l.Id).First(l => l.SellerId == sender.Id).Id;
        }

        var createResponse = await client.PostAsJsonAsync("/api/messages", new CreateMessageDto
        {
            Text = "Initial message",
            SenderId = sender.Id,
            ReceiverId = receiver.Id,
            ListingId = listingId
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var message = await createResponse.Content.ReadFromJsonAsync<MessageDto>();
        Assert.NotNull(message);

        var updateResponse = await client.PutAsJsonAsync($"/api/messages/{message.Id}", new UpdateMessageDto
        {
            Text = "Updated message"
        });

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/messages/{message.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Buyer_CanCreateOrderAndReviewDeliveredListingOnlyOnce()
    {
        using var client = _factory.CreateClient();
        var seller = await RegisterAsync(client, "Seller");
        var buyer = await RegisterAsync(client, "Buyer");

        int listingId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
            var category = new Category { Name = $"Order-Category-{Guid.NewGuid():N}" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var listing = new Listing
            {
                Title = "Order listing",
                CategoryId = category.Id,
                SellerId = seller.Id,
                Price = 250
            };
            db.Listings.Add(listing);
            await db.SaveChangesAsync();
            listingId = listing.Id;
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", buyer.Token);

        var orderResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderDto
        {
            ListingIds = [listingId]
        });

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal(250, order.TotalAmount);
        Assert.Equal(250, order.Items.Single().UnitPrice);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
            var storedOrder = db.Orders.Single(o => o.Id == order.Id);
            storedOrder.Status = "Delivered";
            await db.SaveChangesAsync();
        }

        var reviewResponse = await client.PostAsJsonAsync(
            $"/api/reviews/order/{order.Id}/listing/{listingId}",
            new CreateReviewDto { Rating = 5, Text = "Good product" });

        Assert.Equal(HttpStatusCode.Created, reviewResponse.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync(
            $"/api/reviews/order/{order.Id}/listing/{listingId}",
            new CreateReviewDto { Rating = 4, Text = "Duplicate" });

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Owner_CanUploadAndDownloadListingImage()
    {
        using var client = _factory.CreateClient();
        var seller = await RegisterAsync(client, "ImageSeller");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seller.Token);

        int listingId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
            var category = new Category { Name = $"Image-{Guid.NewGuid():N}" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            var listing = new Listing { Title = "Image listing", CategoryId = category.Id, SellerId = seller.Id, Price = 10 };
            db.Listings.Add(listing);
            await db.SaveChangesAsync();
            listingId = listing.Id;
        }

        using var form = new MultipartFormDataContent();
        using var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(imageContent, "file", "photo.png");

        var upload = await client.PostAsync($"/api/listings/{listingId}/images", form);
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var image = await upload.Content.ReadFromJsonAsync<ImageResponse>();
        Assert.NotNull(image);

        var download = await client.GetAsync($"/api/listings/{listingId}/images/{image!.Id}");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal("image/png", download.Content.Headers.ContentType?.MediaType);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, await download.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Upload_RejectsUnsupportedTypeAndOversizedFile()
    {
        using var client = _factory.CreateClient();
        var seller = await RegisterAsync(client, "ValidationSeller");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seller.Token);

        int listingId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
            var category = new Category { Name = $"Validation-{Guid.NewGuid():N}" };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            var listing = new Listing { Title = "Validation listing", CategoryId = category.Id, SellerId = seller.Id, Price = 10 };
            db.Listings.Add(listing);
            await db.SaveChangesAsync();
            listingId = listing.Id;
        }

        using var invalidForm = new MultipartFormDataContent();
        using var invalidContent = new ByteArrayContent(new byte[] { 1 });
        invalidContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        invalidForm.Add(invalidContent, "file", "notes.txt");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PostAsync($"/api/listings/{listingId}/images", invalidForm)).StatusCode);

        using var largeForm = new MultipartFormDataContent();
        using var largeContent = new ByteArrayContent(new byte[(5 * 1024 * 1024) + 1]);
        largeContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        largeForm.Add(largeContent, "file", "large.png");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PostAsync($"/api/listings/{listingId}/images", largeForm)).StatusCode);
    }

    private sealed record ImageResponse(int Id, int ListingId, int SortOrder);

    private static async Task<AuthResponse> RegisterAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Name = $"{name}-{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "Password123!"
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }
}

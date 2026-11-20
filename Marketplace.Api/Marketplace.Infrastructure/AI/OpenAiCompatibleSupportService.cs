using System.Net.Http.Json;
using System.Text.Json;
using Marketplace.Application.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.AI;

public sealed class OpenAiCompatibleSupportService(
    HttpClient httpClient,
    IOptions<AiOptions> options,
    IOptions<StoreRulesOptions> rules,
    ILogger<OpenAiCompatibleSupportService> logger) : IAiSupportService
{
    public async Task<AiSupportResponse> AskAsync(
        AiSupportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            throw new ArgumentException("Питання не може бути порожнім.");

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.BaseUrl) ||
            string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.Model))
            throw new InvalidOperationException("AI не налаштовано. Вкажіть BaseUrl, ApiKey та Model.");

        var endpoint = $"models/{Uri.EscapeDataString(settings.Model)}:generateContent?key={Uri.EscapeDataString(settings.ApiKey)}";
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = $"Ти помічник маркетплейсу. Відповідай українською. Не вигадуй правила. Правила магазину:\n{rules.Value.Content}" }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = request.Question } }
                    }
                },
                generationConfig = new { temperature = 0.2 }
            })
        };

        try
        {
            using var response = await httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("AI service returned HTTP {StatusCode}: {Body}",
                    (int)response.StatusCode, errorBody);
                throw new HttpRequestException($"AI service returned {(int)response.StatusCode}: {errorBody}");
            }

            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var answer = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(answer))
                throw new InvalidOperationException("AI повернув порожню відповідь.");

            return new AiSupportResponse(answer);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Час очікування відповіді AI вичерпано.", exception);
        }
    }
}

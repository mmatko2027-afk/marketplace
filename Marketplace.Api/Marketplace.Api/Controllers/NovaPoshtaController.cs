using Marketplace.Application.Background;
using Marketplace.Application.Delivery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/delivery/nova-poshta")]
[Authorize]
public sealed class NovaPoshtaController(
    IBackgroundTaskQueue taskQueue,
    IJobStatusStore jobStatusStore,
    IServiceScopeFactory serviceScopeFactory) : ControllerBase
{
    /// <summary>Ставить пошук населених пунктів Нової пошти у фонову чергу й одразу повертає jobId.</summary>
    [HttpGet("cities")]
    [ProducesResponseType<QueuedJobResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchCities(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний запит",
                detail: "Вкажіть назву міста.");
        }

        if (limit is < 1 or > 100)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний запит",
                detail: "Кількість міст має бути від 1 до 100.");
        }

        if (page < 1)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний запит",
                detail: "Номер сторінки має бути не менше 1.");
        }

        var jobId = jobStatusStore.CreateJob();
        var statusStore = jobStatusStore;
        var scopeFactory = serviceScopeFactory;

        await taskQueue.QueueAsync(
            stoppingToken => ProcessSearchCitiesJobAsync(
                jobId,
                query,
                limit,
                page,
                scopeFactory,
                statusStore,
                stoppingToken),
            cancellationToken);

        return Accepted(new QueuedJobResponse(jobId));
    }

    /// <summary>Ставить пошук відділень Нової пошти у фонову чергу й одразу повертає jobId.</summary>
    [HttpGet("warehouses")]
    [ProducesResponseType<QueuedJobResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchWarehouses(
        [FromQuery] string query,
        [FromQuery] string? cityRef,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний запит",
                detail: "Вкажіть назву міста або номер відділення.");
        }

        var searchRequest = new NovaPoshtaSearchRequest(query, cityRef);
        var jobId = jobStatusStore.CreateJob();
        var statusStore = jobStatusStore;
        var scopeFactory = serviceScopeFactory;

        await taskQueue.QueueAsync(
            stoppingToken => ProcessSearchWarehousesJobAsync(
                jobId,
                searchRequest,
                scopeFactory,
                statusStore,
                stoppingToken),
            cancellationToken);

        return Accepted(new QueuedJobResponse(jobId));
    }

    private static async Task ProcessSearchCitiesJobAsync(
        Guid jobId,
        string query,
        int limit,
        int page,
        IServiceScopeFactory scopeFactory,
        IJobStatusStore statusStore,
        CancellationToken cancellationToken)
    {
        statusStore.SetStatus(jobId, BackgroundJobStatus.Processing);

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var novaPoshtaService = scope.ServiceProvider.GetRequiredService<INovaPoshtaService>();
            var cities = await novaPoshtaService.SearchCitiesAsync(query, limit, page, cancellationToken);
            statusStore.SetResult(jobId, cities);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            statusStore.SetError(
                jobId,
                "Виконання задачі скасовано через зупинку сервера.",
                BackgroundJobStatus.Cancelled);
            throw;
        }
        catch (Exception exception)
        {
            statusStore.SetError(jobId, exception.Message, BackgroundJobStatus.Failed);
            throw;
        }
    }

    private static async Task ProcessSearchWarehousesJobAsync(
        Guid jobId,
        NovaPoshtaSearchRequest request,
        IServiceScopeFactory scopeFactory,
        IJobStatusStore statusStore,
        CancellationToken cancellationToken)
    {
        statusStore.SetStatus(jobId, BackgroundJobStatus.Processing);

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var novaPoshtaService = scope.ServiceProvider.GetRequiredService<INovaPoshtaService>();
            var warehouses = await novaPoshtaService.SearchWarehousesAsync(request, cancellationToken);
            statusStore.SetResult(jobId, warehouses);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            statusStore.SetError(
                jobId,
                "Виконання задачі скасовано через зупинку сервера.",
                BackgroundJobStatus.Cancelled);
            throw;
        }
        catch (Exception exception)
        {
            statusStore.SetError(jobId, exception.Message, BackgroundJobStatus.Failed);
            throw;
        }
    }
}
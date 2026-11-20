using Marketplace.Application.AI;
using Marketplace.Application.Background;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/support/ai")]
[Authorize]
public sealed class AiSupportController(
    IBackgroundTaskQueue taskQueue,
    IJobStatusStore jobStatusStore,
    IServiceScopeFactory serviceScopeFactory) : ControllerBase
{
    /// <summary>Ставить запит до AI-помічника (Gemini) у фонову чергу й одразу повертає jobId.</summary>
    [HttpPost("ask")]
    [ProducesResponseType<QueuedJobResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ask(
        AiSupportRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректне питання",
                detail: "Питання не може бути порожнім.");
        }

        var jobId = jobStatusStore.CreateJob();
        var statusStore = jobStatusStore;
        var scopeFactory = serviceScopeFactory;

        await taskQueue.QueueAsync(
            stoppingToken => ProcessAskJobAsync(jobId, request, scopeFactory, statusStore, stoppingToken),
            cancellationToken);

        return Accepted(new QueuedJobResponse(jobId));
    }

    private static async Task ProcessAskJobAsync(
        Guid jobId,
        AiSupportRequest request,
        IServiceScopeFactory scopeFactory,
        IJobStatusStore statusStore,
        CancellationToken cancellationToken)
    {
        statusStore.SetStatus(jobId, BackgroundJobStatus.Processing);

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var aiService = scope.ServiceProvider.GetRequiredService<IAiSupportService>();
            var response = await aiService.AskAsync(request, cancellationToken);
            statusStore.SetResult(jobId, response);
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
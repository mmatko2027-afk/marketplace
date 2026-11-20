using Marketplace.Application.Background;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/background-tasks")]
[Authorize]
public sealed class BackgroundTasksController(
    IBackgroundTaskQueue taskQueue,
    ILogger<BackgroundTasksController> logger) : ControllerBase
{
    /// <summary>Забезпечує створення та додавання фонових задач до черги для їх асинхронного виконання.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        
        var taskId = Guid.NewGuid();

        await taskQueue.QueueAsync(async stoppingToken =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                logger.LogInformation("Фонова задача {TaskId} завершена.", taskId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Фонова задача {TaskId} скасована.", taskId);
            }
        }, cancellationToken);

        return Accepted(new
        {
            taskId,
            status = "queued",
            message = "Задачу поставлено у фонову чергу."
        });
    }
}

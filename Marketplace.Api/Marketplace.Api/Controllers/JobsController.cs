using Marketplace.Application.Background;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public sealed class JobsController(IJobStatusStore jobStatusStore) : ControllerBase
{
    /// <summary>Повертає стан і результат фонової задачі за її ідентифікатором.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<JobStatusDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<JobStatusDto> GetById(Guid id)
    {
        var job = jobStatusStore.GetJob(id);
        if (job is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Фонову задачу не знайдено",
                detail: $"Background job with id '{id}' does not exist.");
        }

        return Ok(new JobStatusDto(
            id,
            job.Status.ToString().ToLowerInvariant(),
            job.Result,
            job.Error));
    }
}
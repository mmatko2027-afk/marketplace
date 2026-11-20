using Marketplace.Application.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/support/ai")]
[Authorize]
public sealed class AiSupportController(IAiSupportService service) : ControllerBase
{
    /// <summary>Відповідає на питання користувача на основі правил магазину.</summary>
    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        AiSupportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.AskAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректне питання", detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "AI-помічник недоступний", detail: exception.Message);
        }
        catch (TimeoutException exception)
        {
            return Problem(statusCode: StatusCodes.Status504GatewayTimeout,
                title: "AI-помічник не відповів вчасно", detail: exception.Message);
        }
        catch (HttpRequestException exception)
        {
            return Problem(statusCode: StatusCodes.Status502BadGateway,
                title: "Помилка AI-сервісу", detail: exception.Message);
        }
    }
}

using Marketplace.Application.Delivery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/delivery/nova-poshta")]
[Authorize]
public sealed class NovaPoshtaController(INovaPoshtaService service) : ControllerBase
{
    /// <summary>Шукає населені пункти для поля з підказками.</summary>
    [HttpGet("cities")]
    public async Task<IActionResult> SearchCities(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await service.SearchCitiesAsync(query, limit, page, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний запит", detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Nova Poshta недоступна", detail: exception.Message);
        }
        catch (TimeoutException exception)
        {
            return Problem(statusCode: StatusCodes.Status504GatewayTimeout,
                title: "Nova Poshta не відповіла вчасно", detail: exception.Message);
        }
    }

    /// <summary>Шукає відділення Нової пошти за містом, адресою або номером.</summary>
    [HttpGet("warehouses")]
    public async Task<IActionResult> SearchWarehouses(
        [FromQuery] string query,
        [FromQuery] string? cityRef,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.SearchWarehousesAsync(
                new NovaPoshtaSearchRequest(query, cityRef), cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Некоректний запит", detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Nova Poshta недоступна", detail: exception.Message);
        }
        catch (TimeoutException exception)
        {
            return Problem(statusCode: StatusCodes.Status504GatewayTimeout,
                title: "Nova Poshta не відповіла вчасно", detail: exception.Message);
        }
    }
}

using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly ListingService _service;

    public ListingsController(ListingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<Listing>>> GetAll()
    {
        var listings = await _service.GetAllAsync();

        return Ok(listings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Listing>> GetById(int id)
    {
        var listing = await _service.GetByIdAsync(id);

        if (listing == null)
        {
            return NotFound();
        }

        return Ok(listing);
    }
}
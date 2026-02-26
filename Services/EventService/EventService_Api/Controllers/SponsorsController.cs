using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SponsorsController(ISponsorService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSponsorDto dto)
    {
        var result = await service.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("by-event/{eventId:guid}")]
    public async Task<IActionResult> GetByEvent(Guid eventId)
    {
        var result = await service.GetByEventIdAsync(eventId);
        return Ok(result);
    }

    [HttpGet("by-tier/{tierId:guid}")]
    public async Task<IActionResult> GetByTier(Guid tierId)
    {
        var result = await service.GetByTierIdAsync(tierId);
        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSponsorDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await service.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}

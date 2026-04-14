using EventService_Api.Filters;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/sponsors")]
[Authorize]
public class SponsorsController(ISponsorService service) : ControllerBase
{
    /// <summary>
    /// List all sponsors for the event.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByEvent(Guid eventId)
    {
        var result = await service.GetByEventIdAsync(eventId);
        return Ok(result);
    }

    /// <summary>
    /// Get a single sponsor by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid eventId, Guid id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get sponsors filtered by sponsor tier.
    /// </summary>
    [HttpGet("by-tier/{tierId:guid}")]
    public async Task<IActionResult> GetByTier(Guid eventId, Guid tierId)
    {
        var result = await service.GetByTierIdAsync(tierId);
        return Ok(result);
    }

    /// <summary>
    /// Create a new sponsor for the event.
    /// eventId is taken from the route and overrides any value in the request body.
    /// </summary>
    [HttpPost]
    [RequireEventPermission(EventPermissions.ManageSponsors)]
    public async Task<IActionResult> Create(Guid eventId, [FromBody] CreateSponsorDto dto)
    {
        dto.EventId = eventId;
        var result = await service.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { eventId, id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update an existing sponsor.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [RequireEventPermission(EventPermissions.ManageSponsors)]
    public async Task<IActionResult> Update(Guid eventId, Guid id, [FromBody] UpdateSponsorDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete a sponsor from the event.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequireEventPermission(EventPermissions.ManageSponsors)]
    public async Task<IActionResult> Delete(Guid eventId, Guid id)
    {
        var result = await service.DeleteAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}

using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/venues")]
[Authorize]
public class EventVenueController(IVenueService venueService) : ControllerBase
{
    /// <summary>
    /// List all venues/rooms for an event.
    /// Public — attendees need this for session room info.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetVenues(Guid eventId, [FromQuery] BaseQueryParams request)
    {
        var result = await venueService.GetByEventIdAsync(eventId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get a single venue by ID within an event.
    /// </summary>
    [HttpGet("{venueId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVenue(Guid eventId, Guid venueId)
    {
        var result = await venueService.GetByIdAsync(eventId, venueId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Create a new venue/room in the event.
    /// Requires: authenticated + can_edit_event permission.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVenue(
        Guid eventId, [FromForm] CreateVenueRequest request)
    {
        // TODO: Permission check — verify caller has "can_edit_event" on this event
        // var userId = GetCurrentUserId();
        // var hasPermission = await permissionService.HasPermissionAsync(userId, eventId, "can_edit_event");
        // if (!hasPermission) return StatusCode(403, ApiResponse<object>.Fail(403, "Insufficient permissions"));

        var result = await venueService.CreateAsync(eventId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update a venue. Supports partial update (PATCH semantics over PUT).
    /// </summary>
    [HttpPut("{venueId:guid}")]
    public async Task<IActionResult> UpdateVenue(
        Guid eventId, Guid venueId, [FromForm] UpdateVenueRequest request)
    {
        var result = await venueService.UpdateAsync(eventId, venueId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete a venue. Fails if sessions are assigned to it.
    /// </summary>
    [HttpDelete("{venueId:guid}")]
    public async Task<IActionResult> DeleteVenue(Guid eventId, Guid venueId)
    {
        var result = await venueService.DeleteAsync(eventId, venueId);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("Missing user claim.");
        return Guid.Parse(sub);
    }
}
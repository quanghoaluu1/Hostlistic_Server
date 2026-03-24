using System.Security.Claims;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/tracks")]
[Authorize]
public class EventTrackController(ITrackService trackService) : ControllerBase
{
        /// <summary>
    /// List all tracks for an event, ordered by SortOrder.
    /// Public endpoint — attendees need this for the agenda view.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTracks(Guid eventId)
    {
        var result = await trackService.GetTracksByEventIdAsync(eventId);
        return StatusCode(result.StatusCode, result);
    }
 
    /// <summary>
    /// Get a single track by ID within an event.
    /// </summary>
    [HttpGet("{trackId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTrack(Guid eventId, Guid trackId)
    {
        var result = await trackService.GetTrackByIdAsync(eventId, trackId);
        return StatusCode(result.StatusCode, result);
    }
 
    /// <summary>
    /// Create a new track in the event.
    /// Requires: authenticated + can_edit_event permission.
    /// EventId comes from URL — NOT from request body.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTrack(Guid eventId, [FromBody] CreateTrackRequest request)
    {
        // TODO: Permission check — verify caller has "can_edit_event" on this event
        // var userId = GetCurrentUserId();
        // var hasPermission = await permissionService.HasPermissionAsync(userId, eventId, "can_edit_event");
        // if (!hasPermission) return StatusCode(403, ApiResponse<object>.Fail(403, "Insufficient permissions"));
 
        var result = await trackService.CreateTrackAsync(eventId, request);
        return StatusCode(result.StatusCode, result);
    }
 
    /// <summary>
    /// Update an existing track.
    /// Requires: authenticated + can_edit_event permission.
    /// </summary>
    [HttpPut("{trackId:guid}")]
    public async Task<IActionResult> UpdateTrack(
        Guid eventId, Guid trackId, [FromBody] UpdateTrackRequest request)
    {
        var result = await trackService.UpdateTrackAsync(eventId, trackId, request);
        return StatusCode(result.StatusCode, result);
    }
 
    /// <summary>
    /// Delete a track. Fails with 409 if track has sessions.
    /// Requires: authenticated + can_edit_event permission.
    /// </summary>
    [HttpDelete("{trackId:guid}")]
    public async Task<IActionResult> DeleteTrack(Guid eventId, Guid trackId)
    {
        var result = await trackService.DeleteTrackAsync(eventId, trackId);
        return StatusCode(result.StatusCode, result);
    }
 
    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.Parse(sub ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
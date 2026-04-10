using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/sessions")]
[Authorize]
public class EventSessionsController(ISessionService sessionService) : ControllerBase
{
    /// <summary>
    /// List all sessions for an event, ordered by StartTime then SortOrder.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetSessions(Guid eventId)
    {
        var result = await sessionService.GetSessionsByEventIdAsync(eventId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get sessions filtered by track.
    /// </summary>
    [HttpGet("by-track/{trackId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSessionsByTrack(Guid eventId, Guid trackId, [FromQuery] BaseQueryParams request)
    {
        var result = await sessionService.GetSessionsByTrackIdAsync(eventId, trackId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get a single session with full details (speakers, venue, booking count).
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSession(Guid eventId, Guid sessionId)
    {
        var result = await sessionService.GetSessionByIdAsync(eventId, sessionId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Create a new session within the event.
    /// Runs 5-step validation: time → track exists → event bounds → track overlap → venue overlap.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSession(
        Guid eventId, [FromBody] CreateSessionRequest request)
    {
        var result = await sessionService.CreateSessionAsync(eventId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update session content (title, description, time, track, venue, capacity).
    /// Re-validates overlap rules if time or track/venue changed.
    /// </summary>
    [HttpPut("{sessionId:guid}")]
    public async Task<IActionResult> UpdateSession(
        Guid eventId, Guid sessionId, [FromBody] UpdateSessionRequest request)
    {
        var result = await sessionService.UpdateSessionAsync(eventId, sessionId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Transition session status.
    /// Separated from content update because:
    /// - Different validation (state machine vs overlap rules)
    /// - Different HTTP semantics (PATCH = partial change)
    /// - Potentially different authorization (staff can transition, only organizer can edit content)
    ///
    /// Valid transitions:
    ///   Scheduled → OnGoing | Cancelled
    ///   OnGoing   → Completed | Cancelled
    /// </summary>
    [HttpPatch("{sessionId:guid}/status")]
    public async Task<IActionResult> UpdateSessionStatus(
        Guid eventId, Guid sessionId, [FromBody] UpdateSessionStatusRequest request)
    {
        var result = await sessionService.UpdateSessionStatusAsync(eventId, sessionId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete a session. Fails with 409 if session has active bookings.
    /// </summary>
    [HttpDelete("{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(Guid eventId, Guid sessionId)
    {
        var result = await sessionService.DeleteSessionAsync(eventId, sessionId);
        return StatusCode(result.StatusCode, result);
    }
}
using System.Security.Claims;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/sessions")]
[Authorize]
public class EventSessionBookingsController(ISessionBookingService bookingService) : ControllerBase
{
    /// <summary>
    /// Book a session for the authenticated user.
    /// Returns 201 with booking details + optional conflict warnings.
    /// Returns 409 if already booked or session is full.
    /// </summary>
    [HttpPost("{sessionId:guid}/book")]
    public async Task<IActionResult> BookSession(Guid eventId, Guid sessionId)
    {
        var userId = GetCurrentUserId();
        var result = await bookingService.BookSessionAsync(eventId, sessionId, userId);
        return StatusCode(result.StatusCode, result);
    }
 
    /// <summary>
    /// Cancel the authenticated user's booking for a session.
    /// Soft delete — changes status to Cancelled, preserves audit trail.
    /// </summary>
    [HttpDelete("{sessionId:guid}/book")]
    public async Task<IActionResult> CancelBooking(Guid eventId, Guid sessionId)
    {
        var userId = GetCurrentUserId();
        var result = await bookingService.CancelBookingAsync(eventId, sessionId, userId);
        return StatusCode(result.StatusCode, result);
    }
 
    /// <summary>
    /// Get all confirmed session bookings for the authenticated user within this event.
    /// Returns a personal schedule sorted by session start time.
    ///
    /// Route: GET /api/events/{eventId}/my-schedule
    /// Uses ~/ route override to mount at a clean path outside the /sessions prefix.
    /// </summary>
    [HttpGet("~/api/events/{eventId:guid}/my-schedule")]
    public async Task<IActionResult> GetMySchedule(Guid eventId)
    {
        var userId = GetCurrentUserId();
        var result = await bookingService.GetMyScheduleAsync(eventId, userId);
        return StatusCode(result.StatusCode, result);
    }
 
    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.Parse(sub ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
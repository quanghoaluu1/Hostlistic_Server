using System.Security.Claims;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

// DEPRECATED: Check-in management has moved to BookingService (/api/checkin).
// These endpoints are kept for backward compatibility during migration.
// They will be removed once YARP routing is updated in Phase 5.
[ApiController]
[Route("api/[controller]")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    // DEPRECATED: Use GET /api/checkin/{checkInId} in BookingService instead.
    [HttpGet("{checkInId:guid}")]
    [Obsolete("Use BookingService GET /api/checkin/{checkInId}")]
    public async Task<IActionResult> GetCheckInById(Guid checkInId)
    {
        var result = await _checkInService.GetCheckInByIdAsync(checkInId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // DEPRECATED: Use GET /api/checkin/event/{eventId} in BookingService instead.
    [HttpGet("event/{eventId:guid}")]
    [Obsolete("Use BookingService GET /api/checkin/event/{eventId}")]
    public async Task<IActionResult> GetCheckInsByEventId(Guid eventId)
    {
        var result = await _checkInService.GetCheckInsByEventIdAsync(eventId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // DEPRECATED: Use GET /api/checkin/session/{sessionId} in BookingService instead.
    [HttpGet("session/{sessionId:guid}")]
    [Obsolete("Use BookingService GET /api/checkin/session/{sessionId}")]
    public async Task<IActionResult> GetCheckInsBySessionId(Guid sessionId)
    {
        var result = await _checkInService.GetCheckInsBySessionIdAsync(sessionId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // DEPRECATED: Use GET /api/checkin/event/{eventId}/ticket/{ticketId} in BookingService instead.
    [HttpGet("ticket/{ticketId:guid}")]
    [Obsolete("Use BookingService GET /api/checkin/event/{eventId}/ticket/{ticketId}")]
    public async Task<IActionResult> GetCheckInByTicketId(Guid ticketId)
    {
        var result = await _checkInService.GetCheckInByTicketIdAsync(ticketId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // DEPRECATED: Use POST /api/checkin/scan in BookingService instead.
    [Authorize]
    [HttpPost]
    [Obsolete("Use BookingService POST /api/checkin/scan")]
    public async Task<IActionResult> CreateCheckIn([FromBody] CreateCheckInRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _checkInService.CreateCheckInAsync(userId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{checkInId:guid}")]
    public async Task<IActionResult> UpdateCheckIn(Guid checkInId, [FromBody] UpdateCheckInRequest request)
    {
        var result = await _checkInService.UpdateCheckInAsync(checkInId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{checkInId:guid}")]
    public async Task<IActionResult> DeleteCheckIn(Guid checkInId)
    {
        var result = await _checkInService.DeleteCheckInAsync(checkInId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}

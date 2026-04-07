using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/attendees")]
[Authorize]
public class AttendeesController(IAttendeeService attendeeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAttendees(
        Guid eventId,
        [FromQuery] AttendeeListRequest request,
        CancellationToken ct)
    {
        var result = await attendeeService.GetAttendeesAsync(eventId, request, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(Guid eventId, CancellationToken ct)
    {
        var result = await attendeeService.GetAttendeeSummaryAsync(eventId, ct);
        return StatusCode(result.StatusCode, result);
    }
}

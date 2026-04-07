using System.Security.Claims;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/checkin")]
[Authorize]
public class CheckInController(ICheckInService checkInService) : ControllerBase
{
    [HttpPost("scan")]
    public async Task<IActionResult> Scan(
        [FromBody] CheckInScanRequest request,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var staffUserId))
            return Unauthorized();

        var result = await checkInService.ScanAsync(request, staffUserId, ct);

        return result.StatusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            404 => NotFound(result),
            409 => Conflict(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetEventCheckIns(Guid eventId, CancellationToken ct)
    {
        var result = await checkInService.GetEventCheckInsAsync(eventId, ct);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("event/{eventId:guid}/stats")]
    public async Task<IActionResult> GetEventCheckInStats(Guid eventId, CancellationToken ct)
    {
        var result = await checkInService.GetEventCheckInStatsAsync(eventId, ct);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("event/{eventId:guid}/ticket/{ticketId:guid}")]
    public async Task<IActionResult> GetTicketCheckInStatus(Guid eventId, Guid ticketId, CancellationToken ct)
    {
        var result = await checkInService.GetTicketCheckInStatusAsync(eventId, ticketId, ct);
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}

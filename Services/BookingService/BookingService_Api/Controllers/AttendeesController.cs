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
    
    [HttpGet("/email-recipients")]
    public async Task<IActionResult> GetEmailRecipients(
        Guid eventId,
        [FromQuery] int recipientGroup = 0,
        [FromQuery] List<Guid>? ticketTypeIds = null,
        [FromQuery] List<Guid>? specificUserIds = null,
        [FromQuery] DateTime? purchasedAfter = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GetEmailRecipientsRequest
        {
            RecipientGroup  = recipientGroup,
            TicketTypeIds   = ticketTypeIds,
            SpecificUserIds = specificUserIds,
            PurchasedAfter  = purchasedAfter
        };

        var result = await attendeeService.GetRecipientsAsync(
            eventId, request, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }
}

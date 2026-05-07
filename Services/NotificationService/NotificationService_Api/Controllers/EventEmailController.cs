using Common;
using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.Interfaces;

namespace NotificationService_Api.Controllers;

/// <summary>
/// Internal endpoint called by EventService (via HTTP) when an event is completed.
/// No JWT auth required — this is an internal service-to-service call.
/// </summary>
[ApiController]
[Route("api/event-email")]
public class EventEmailController(
    IThankYouEmailService thankYouEmailService,
    ILogger<EventEmailController> logger) : ControllerBase
{
    /// <summary>
    /// Trigger Thank-You email campaign for all checked-in attendees of a completed event.
    /// Called by EventService immediately after marking the event as Completed.
    /// Idempotent: safe to call multiple times — duplicate campaigns are blocked.
    /// </summary>
    /// <param name="eventId">The completed event ID.</param>
    /// <param name="request">Event title, organizer ID, and completion timestamp.</param>
    [HttpPost("{eventId:guid}/thank-you")]
    public async Task<IActionResult> TriggerThankYouEmail(
        Guid eventId,
        [FromBody] TriggerThankYouRequest request,
        CancellationToken ct)
    {
        logger.LogInformation(
            "EventEmailController: POST /api/event-email/{EventId}/thank-you received. " +
            "EventTitle: '{Title}', OrganizerId: {OrganizerId}.",
            eventId, request.EventTitle, request.OrganizerId);

        var result = await thankYouEmailService.SendThankYouEmailsAsync(
            eventId,
            request.EventTitle,
            request.OrganizerId,
            request.CompletedAt,
            ct);

        return StatusCode(result.StatusCode, result);
    }
}

public sealed record TriggerThankYouRequest(
    string EventTitle,
    Guid OrganizerId,
    DateTime CompletedAt);

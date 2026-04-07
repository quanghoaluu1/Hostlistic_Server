using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}")]
public class EventLineupController(ILineupService lineupService) : ControllerBase
{
    /// <summary>
    /// Get the public speaker/talent lineup for an event, shaped for attendee display.
    /// Separates event-wide talents (SessionId null) from session-scoped talents (SessionId not null).
    /// AllowAnonymous — no authentication required.
    /// </summary>
    [HttpGet("lineup")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicLineup(Guid eventId)
    {
        var result = await lineupService.GetPublicLineupAsync(eventId);
        return StatusCode(result.StatusCode, result);
    }
}

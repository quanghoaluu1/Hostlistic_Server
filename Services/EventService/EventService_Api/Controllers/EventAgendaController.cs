using System.Security.Claims;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}")]
public class EventAgendaController(IAgendaService agendaService): ControllerBase
{
    [HttpGet("agenda")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAgenda(Guid eventId)
    {
        // Extract userId if authenticated — for IsBookedByCurrentUser
        Guid? currentUserId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");
            if (Guid.TryParse(sub, out var parsed))
                currentUserId = parsed;
        }
 
        var result = await agendaService.GetAgendaAsync(eventId, currentUserId);
        return StatusCode(result.StatusCode, result);
    }
}
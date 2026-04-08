using System.Security.Claims;
using BookingService_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class MeController(IRegisteredEventService registeredEventService) : ControllerBase
{
    [HttpGet("registered-events")]
    public async Task<IActionResult> GetRegisteredEvents()
    {
        var userId = GetCurrentUserId();
        var result = await registeredEventService.GetMyRegisteredEventsAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.Parse(sub ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
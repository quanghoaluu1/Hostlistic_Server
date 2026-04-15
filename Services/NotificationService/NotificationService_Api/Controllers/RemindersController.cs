using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.Dtos;
using NotificationService_Application.Interfaces;

namespace NotificationService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RemindersController(IReminderService reminderService) : ControllerBase
{
    [HttpPost("{eventId:guid}/setup")]
    public async Task<IActionResult> Setup(
        Guid eventId,
        [FromBody] SetupAutoRemindersRequest request,
        CancellationToken cancellationToken)
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await reminderService.SetupAutoRemindersAsync(
            eventId, organizerId, request, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{eventId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var result = await reminderService.CancelAutoRemindersAsync(eventId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
using System.Security.Claims;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;


[ApiController]
[Route("api/events/{eventId:guid}/export")]
[Authorize]
public sealed class ExportController(
    IExportService exportService,
    IEventPermissionClient permissionClient
    ) : ControllerBase
{
    [HttpGet("attendees")]
    public async Task<IActionResult> ExportAttendees(
        Guid eventId,
        [FromQuery] ExportFormat format = ExportFormat.Xlsx,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        // Cross-service permission check → EventService
        var hasPermission = await permissionClient.HasPermissionAsync(
            eventId, userId, "can_export_data", ct);

        if (!hasPermission)
            return StatusCode(403, new { message = "You do not have permission to export data." });

        var result = await exportService.ExportAttendeesAsync(eventId, format, ct);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return File(result.Data!.FileContent, result.Data.ContentType, result.Data.FileName);
    }
    
    [HttpGet("orders")]
    public async Task<IActionResult> ExportOrders(
        Guid eventId,
        [FromQuery] ExportFormat format = ExportFormat.Xlsx,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        var hasPermission = await permissionClient.HasPermissionAsync(
            eventId, userId, "can_export_data", ct);

        if (!hasPermission)
            return StatusCode(403, new { message = "You do not have permission to export data." });

        var result = await exportService.ExportOrdersAsync(eventId, format, ct);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return File(result.Data!.FileContent, result.Data.ContentType, result.Data.FileName);
    }

    
    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.Parse(sub ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
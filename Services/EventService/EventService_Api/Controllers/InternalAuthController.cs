using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/internal/auth")]
[AllowAnonymous] // Internal network only — no JWT needed for service-to-service
public sealed class InternalAuthController(
    IEventAuthorizationService authService) : ControllerBase
{
    /// <summary>
    /// Check if a user has a specific permission on an event.
    /// Called by other microservices before performing permission-gated actions.
    ///
    /// GET /api/internal/auth/check?eventId=xxx&userId=yyy&permission=can_export_data
    /// Returns: { "granted": true/false }
    /// </summary>
    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission(
        [FromQuery] Guid eventId,
        [FromQuery] Guid userId,
        [FromQuery] string permission,
        CancellationToken ct)
    {
        if (eventId == Guid.Empty || userId == Guid.Empty || string.IsNullOrWhiteSpace(permission))
            return BadRequest(new { granted = false, error = "Missing required parameters." });

        var granted = await authService.HasPermissionAsync(eventId, userId, permission, ct);

        return Ok(new { granted });
    }

    /// <summary>
    /// Get all permissions for a user on an event.
    /// Useful when BookingService needs to know multiple permissions at once.
    ///
    /// GET /api/internal/auth/permissions?eventId=xxx&userId=yyy
    /// Returns: { "isOwner": true, "permissions": { "can_export_data": true, ... } }
    /// </summary>
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(
        [FromQuery] Guid eventId,
        [FromQuery] Guid userId,
        CancellationToken ct)
    {
        if (eventId == Guid.Empty || userId == Guid.Empty)
            return BadRequest(new { error = "Missing required parameters." });

        var isOwner = await authService.IsEventOwnerAsync(eventId, userId, ct);
        var permissions = await authService.GetUserPermissionsAsync(eventId, userId, ct);

        return Ok(new
        {
            isOwner,
            permissions
        });
    }
}
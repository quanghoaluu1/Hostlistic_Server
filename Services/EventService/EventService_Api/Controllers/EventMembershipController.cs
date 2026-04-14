using Common;
using EventService_Api.Extensions;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventMembershipController(IEventAuthorizationService authService) : ControllerBase
{
    /// <summary>
    /// Returns the caller's effective permissions for the given event.
    /// Used by the frontend to conditionally render organizer-dashboard UI elements.
    /// This is a UX hint only — security is enforced server-side by RequireEventPermissionAttribute.
    ///
    /// Always returns 200, even for non-members (isOwner: false, role: null, permissions: {}).
    /// </summary>
    [HttpGet("{eventId:guid}/my-permissions")]
    public async Task<IActionResult> GetMyPermissions(Guid eventId)
    {
        var ct = HttpContext.RequestAborted;
        var userId = User.GetUserId();

        var isOwnerTask = authService.IsEventOwnerAsync(eventId, userId, ct);
        var permissionsTask = authService.GetUserPermissionsAsync(eventId, userId, ct);
        var roleTask = authService.GetUserRoleAsync(eventId, userId, ct);

        await Task.WhenAll(isOwnerTask, permissionsTask, roleTask);

        var isOwner = isOwnerTask.Result;
        var permissions = permissionsTask.Result;
        // Event owner is not stored in EventTeamMember; reflect "Organizer" role for them.
        var role = isOwner ? "Organizer" : roleTask.Result;

        var dto = new EventPermissionDto(isOwner, role, permissions);
        return StatusCode(200, ApiResponse<EventPermissionDto>.Success(200, "Permissions retrieved successfully.", dto));
    }
}

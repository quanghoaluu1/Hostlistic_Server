using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/team-members")]
public class TeamMemberController(ITeamMemberService teamMemberService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Invite(Guid eventId, [FromBody] InviteTeamMemberRequest request)
    {
        var result = await teamMemberService.InviteAsync(eventId, GetCurrentUserId(), GetCurrentUserName(), request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetByEventId(Guid eventId)
    {
        var result = await teamMemberService.GetByEventIdAsync(eventId, GetCurrentUserId());
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("invitations/{token}/accept")]
    public async Task<IActionResult> AcceptByToken(Guid eventId, string token)
    {
        var result = await teamMemberService.AcceptByTokenAsync(token);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("invitations/{token}/decline")]
    public async Task<IActionResult> DeclineByToken(Guid eventId, string token)
    {
        var result = await teamMemberService.DeclineByTokenAsync(token);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPatch("my-invitation")]
    public async Task<IActionResult> RespondByUser(Guid eventId, [FromBody] RespondToInvitationRequest request)
    {
        var result = await teamMemberService.RespondByUserAsync(eventId, GetCurrentUserId(), request.Action);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid eventId, Guid memberId)
    {
        var result = await teamMemberService.RemoveMemberAsync(eventId, memberId, GetCurrentUserId());
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPatch("{memberId:guid}/permissions")]
    public async Task<IActionResult> UpdatePermissions(
        Guid eventId,
        Guid memberId,
        [FromBody] UpdateMemberPermissionsRequest request)
    {
        var result = await teamMemberService.UpdatePermissionsAsync(eventId, memberId, GetCurrentUserId(), request.Permissions);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(value!);
    }

    private string? GetCurrentUserName() => User.FindFirst(ClaimTypes.Name)?.Value;
}

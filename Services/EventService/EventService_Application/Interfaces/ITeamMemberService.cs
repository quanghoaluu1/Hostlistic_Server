using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ITeamMemberService
{
    Task<ApiResponse<TeamMemberDto>> InviteAsync(Guid eventId, Guid currentUserId, string? currentUserName, InviteTeamMemberRequest request);
    Task<ApiResponse<List<TeamMemberDto>>> GetByEventIdAsync(Guid eventId, Guid currentUserId);
    Task<ApiResponse<string>> AcceptByTokenAsync(string token);
    Task<ApiResponse<string>> DeclineByTokenAsync(string token);
    Task<ApiResponse<string>> RespondByUserAsync(Guid eventId, Guid currentUserId, string action);
    Task<ApiResponse<string>> RemoveMemberAsync(Guid eventId, Guid memberId, Guid currentUserId);
    Task<ApiResponse<TeamMemberDto>> UpdatePermissionsAsync(Guid eventId, Guid memberId, Guid currentUserId, Dictionary<string, bool> permissions);
}

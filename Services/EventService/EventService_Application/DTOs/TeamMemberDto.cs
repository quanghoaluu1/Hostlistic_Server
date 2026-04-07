using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public record InviteTeamMemberRequest(
    Guid UserId,
    EventRole Role,
    string? CustomTitle,
    Dictionary<string, bool>? Permissions,
    string? UserFullName,
    string? UserEmail
);

public record RespondToInvitationRequest(string Action); // "accept" or "decline"

public record UpdateMemberPermissionsRequest(Dictionary<string, bool> Permissions);

public record TeamMemberDto(
    Guid Id,
    Guid UserId,
    Guid EventId,
    string Role,
    string? CustomTitle,
    Dictionary<string, bool> Permissions,
    string Status,
    DateTime InvitedAt,
    DateTime? JoinedAt,
    DateTime? DeclinedAt,
    Guid InvitedByUserId,
    string? UserFullName,
    string? UserEmail
);

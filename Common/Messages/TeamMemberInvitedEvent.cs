namespace Common.Messages;

public record TeamMemberInvitedEvent(
    Guid EventId,
    string EventTitle,
    Guid InvitedUserId,
    string InvitedUserEmail,
    string InvitedUserName,
    string Role,
    string? CustomTitle,
    string InviteToken,
    DateTime InviteTokenExpiry,
    Guid InvitedByUserId,
    string InvitedByUserName
);

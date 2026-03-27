namespace NotificationService_Application.Dtos;

public record InviteMemberEmailRequest(
    Guid EventId,
    string EventTitle,
    string InvitedUserEmail,
    string InvitedUserName,
    string Role,
    string? CustomTitle,
    string InviteToken,
    DateTime InviteTokenExpiry,
    string InvitedByUserName
);

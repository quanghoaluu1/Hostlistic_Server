using EventService_Domain.Constants;
using EventService_Domain.Enums;

namespace EventService_Test.Helpers.TestDataBuilders;

public static class TeamMemberBuilder
{
    public static EventTeamMember CreateActiveMember(
        Guid? id = null,
        Guid? userId = null,
        Guid? eventId = null,
        EventRole role = EventRole.Staff)
    {
        return new EventTeamMember
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            Role = role,
            Status = EventMemberStatus.Active,
            Permissions = EventPermissions.GetPreset(role),
            InvitedAt = DateTime.UtcNow.AddDays(-1),
            InvitedByUserId = Guid.NewGuid()
        };
    }

    public static EventTeamMember CreateInvitedMember(
        Guid? id = null,
        Guid? userId = null,
        Guid? eventId = null,
        EventRole role = EventRole.CoOrganizer)
    {
        return new EventTeamMember
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            Role = role,
            Status = EventMemberStatus.Invited,
            Permissions = EventPermissions.GetPreset(role),
            InvitedAt = DateTime.UtcNow,
            InviteToken = Guid.NewGuid().ToString("N"),
            InviteTokenExpiry = DateTime.UtcNow.AddDays(3),
            InvitedByUserId = Guid.NewGuid()
        };
    }

    public static InviteTeamMemberRequest InviteRequest(
        Guid? userId = null,
        EventRole role = EventRole.Staff,
        string? email = "member@example.com",
        string? fullName = "John Member") => new InviteTeamMemberRequest(
        UserId: userId ?? Guid.NewGuid(),
        Role: role,
        CustomTitle: null,
        Permissions: null,
        UserFullName: fullName,
        UserEmail: email);
}

using System.ComponentModel.DataAnnotations.Schema;
using EventService_Domain.Constants;
using EventService_Domain.Enums;
using EventService_Domain.Exceptions;

namespace EventService_Domain.Entities;

public class EventTeamMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public EventRole Role { get; set; }
    public string? CustomTitle { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public Dictionary<string, bool> Permissions { get; set; } = new();

    public EventMemberStatus Status { get; set; } = EventMemberStatus.Invited;
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? JoinedAt { get; set; }

    // Invite token (email-link flow)
    public string? InviteToken { get; set; }
    public DateTime? InviteTokenExpiry { get; set; }

    // Audit
    public Guid InvitedByUserId { get; set; }
    public DateTime? DeclinedAt { get; set; }

    // Event-Carried State Transfer: denormalized at invite time to avoid cross-service calls when listing
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }

    // Navigation property to parent
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;

    public static EventTeamMember CreateInvitation(
        Guid eventId,
        Guid userId,
        EventRole role,
        Guid invitedByUserId,
        string? customTitle,
        Dictionary<string, bool>? customPermissions,
        string? userFullName = null,
        string? userEmail = null)
    {
        if (role is EventRole.Organizer or EventRole.Attendee)
            throw new DomainException($"Cannot invite with role '{role}'. Valid roles: CoOrganizer, Staff, Volunteer.");

        var permissions = EventPermissions.GetPreset(role);

        if (customPermissions is not null)
        {
            foreach (var (key, value) in customPermissions)
            {
                if (permissions.ContainsKey(key))
                    permissions[key] = value;
            }
        }

        return new EventTeamMember
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            UserId = userId,
            Role = role,
            CustomTitle = customTitle,
            Permissions = permissions,
            Status = EventMemberStatus.Invited,
            InvitedAt = DateTime.UtcNow,
            InviteToken = Guid.NewGuid().ToString("N"),
            InviteTokenExpiry = DateTime.UtcNow.AddDays(3),
            InvitedByUserId = invitedByUserId,
            UserFullName = userFullName,
            UserEmail = userEmail,
        };
    }

    public void AcceptInvitation()
    {
        if (Status != EventMemberStatus.Invited)
            throw new DomainException("Invitation cannot be accepted: it is not in Invited status.");

        if (InviteTokenExpiry.HasValue && InviteTokenExpiry.Value < DateTime.UtcNow)
            throw new DomainException("Invitation token has expired.");

        Status = EventMemberStatus.Active;
        JoinedAt = DateTime.UtcNow;
        InviteToken = null;
        InviteTokenExpiry = null;
    }

    public void DeclineInvitation()
    {
        if (Status != EventMemberStatus.Invited)
            throw new DomainException("Invitation cannot be declined: it is not in Invited status.");

        Status = EventMemberStatus.Declined;
        DeclinedAt = DateTime.UtcNow;
        InviteToken = null;
        InviteTokenExpiry = null;
    }
}

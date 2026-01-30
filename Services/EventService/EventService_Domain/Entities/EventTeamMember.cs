using System.ComponentModel.DataAnnotations.Schema;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class EventTeamMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public EventRole Role { get; set; }
    public string? CustomTitle { get; set; } = string.Empty;
    [Column(TypeName = "jsonb")]
    public Dictionary<string, bool> Permissions { get; set; } = new Dictionary<string, bool>(); //{"can_checkin": true, "can_edit_event": false}
    public EventMemberStatus Status { get; set; } = EventMemberStatus.Invited;
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? JoinedAt { get; set; }
    
    // Navigation property to parent
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
}
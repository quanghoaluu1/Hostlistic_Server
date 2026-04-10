using Common;
using System.ComponentModel.DataAnnotations.Schema;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class SessionEngagementRestriction     : BaseClass
{
    public Guid EventId { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public EngagementRestrictionScope Scope { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Reason { get; set; }
    public Guid CreatedByUserId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; } = null!;
}

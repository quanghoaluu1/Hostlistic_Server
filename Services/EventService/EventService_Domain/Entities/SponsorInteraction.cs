using System.ComponentModel.DataAnnotations.Schema;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class SponsorInteraction
{
    public Guid Id { get; set; }
    public Guid SponsorId { get; set; }
    public Guid UserId { get; set; }
    public InteractionType InteractionType { get; set; }
    public DateTime InteractionDate { get; set; } = DateTime.UtcNow;
    
    // Navigation property to parent
    [ForeignKey("SponsorId")]
    public virtual Sponsor Sponsor { get; set; } = null!;
}
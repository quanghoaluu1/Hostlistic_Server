using System.ComponentModel.DataAnnotations.Schema;

namespace EventService_Domain.Entities;

public class SponsorTier
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; } //Càng nhỏ càng ưu tiên
    
    // Navigation property to parent
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
    
    // Navigation properties to children
    public ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();
}
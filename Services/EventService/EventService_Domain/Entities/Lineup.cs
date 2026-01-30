using System.ComponentModel.DataAnnotations.Schema;

namespace EventService_Domain.Entities;

public class Lineup
{
    public Guid Id { get; set; }
    public Guid TalentId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid EventId { get; set; }
    
    // Navigation properties to parent
    [ForeignKey("TalentId")]
    public virtual Talent Talent { get; set; } = null!;
    
    [ForeignKey("SessionId")]
    public virtual Session? Session { get; set; }
    
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
}
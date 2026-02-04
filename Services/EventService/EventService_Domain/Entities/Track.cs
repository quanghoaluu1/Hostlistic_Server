using System.ComponentModel.DataAnnotations.Schema;

namespace EventService_Domain.Entities;

public class Track
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    
    
    // Navigation property to parent
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
    
    // Navigation properties to children
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
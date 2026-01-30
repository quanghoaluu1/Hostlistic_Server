using System.ComponentModel.DataAnnotations.Schema;
using Common;

namespace EventService_Domain.Entities;

public class Feedback : BaseClass
{
    public Guid EventId { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    
    // Navigation properties to parent
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
    
    [ForeignKey("SessionId")]
    public virtual Session Session { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations.Schema;

namespace EventService_Domain.Entities;

public class Venue
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public string? LayoutUrl { get; set; } = string.Empty;
    public string? LayoutPublicId { get; set; }
    
    // Navigation properties to children
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
    
    public static Venue Create(Guid eventId, string name, string? description, int capacity)
    {
        return new Venue
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            Name = name,
            Description = description,
            Capacity = capacity
        };
    }
}
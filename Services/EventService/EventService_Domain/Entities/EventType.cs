using EventService_Domain.Entities;

namespace EventService_Domain;

public class EventType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
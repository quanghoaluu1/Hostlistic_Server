namespace EventService_Domain.Entities;

public class Venue
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? LayoutUrl { get; set; } = string.Empty;
    
    // Navigation properties to children
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
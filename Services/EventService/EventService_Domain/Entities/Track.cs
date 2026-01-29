namespace EventService_Domain.Entities;

public class Track
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
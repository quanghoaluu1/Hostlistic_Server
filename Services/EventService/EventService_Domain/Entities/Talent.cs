namespace EventService_Domain.Entities;

public class Talent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Organization { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    
    public ICollection<Talent> Talents { get; set; } = new List<Talent>();
}
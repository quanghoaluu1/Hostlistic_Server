namespace EventService_Domain.Entities;

public class Sponsor
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; } = string.Empty;
    public Guid TierId { get; set; }
    
    public ICollection<SponsorInteraction> SponsorInteractions { get; set; } = new List<SponsorInteraction>();
}
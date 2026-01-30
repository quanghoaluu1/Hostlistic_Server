using System.ComponentModel.DataAnnotations.Schema;

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
    
    // Navigation properties to parent
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
    
    [ForeignKey("TierId")]
    public virtual SponsorTier Tier { get; set; } = null!;
    
    // Navigation properties to children
    public ICollection<SponsorInteraction> SponsorInteractions { get; set; } = new List<SponsorInteraction>();
}
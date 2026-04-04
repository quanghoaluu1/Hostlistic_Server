    using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Common;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class Event : BaseClass
{
    public Guid OrganizerId { get; set; }
    public string? Title { get; set; } = string.Empty;
    public RichTextContent? Description { get; set; }
    public EventMode? EventMode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; } = string.Empty;
    public Guid? EventTypeId { get; set; }
    public string? CoverImageUrl { get; set; } = string.Empty;
    public string? CoverImagePublicId { get; set; } = string.Empty;
    public int? TotalCapacity { get; set; }
    public bool? IsPublic { get; set; } = false;
    public EventStatus EventStatus { get; set; } = EventStatus.Draft;
    
    // Navigation properties to parent
    [ForeignKey("EventTypeId")]
    public virtual EventType? EventType { get; set; }
    
    
    // Navigation properties to children
    public ICollection<EventTeamMember> EventTeamMembers { get; set; } = new List<EventTeamMember>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Lineup> Lineups { get; set; } = new List<Lineup>();
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    public ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Venue> Venues { get; set; } = [];
    public ICollection<EventDay> EventDays { get; set; } = new List<EventDay>();
}

public class RichTextContent
{
    public string Type { get; set; } // "doc"
    public JsonElement Content { get; set; } // Các node nội dung
}
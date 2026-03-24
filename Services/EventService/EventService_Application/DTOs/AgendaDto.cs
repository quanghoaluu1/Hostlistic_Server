using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public class AgendaResponse
{
    public Guid EventId { get; set; }
    public DateTime? EventStartDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public List<AgendaTrackDto> Tracks { get; set; } = [];
}
 
public class AgendaTrackDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<AgendaSessionDto> Sessions { get; set; } = [];
}
 
public class AgendaSessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? TotalCapacity { get; set; }
    public int BookedCount { get; set; }
    public bool IsFull { get; set; }
    public string? VenueName { get; set; }
    public SessionStatus Status { get; set; }
    public List<SpeakerBriefDto> Speakers { get; set; } = [];
    public bool IsBookedByCurrentUser { get; set; }
}
 
public class SpeakerBriefDto
{
    public Guid TalentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
namespace AIService_Application.DTOs.Responses;

public class EventDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? EventMode { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public string? EventStatus { get; set; }
    public int TotalCapacity { get; set; }
    public bool IsPublic { get; set; }
    public VenueDetailDto? Venue { get; set; }
    public TrackDetailDto[] Tracks { get; set; } = [];
}

public record VenueDetailDto(Guid Id, string Name, string Address, string Capacity);

public record SessionDetailDto(Guid Id, string Title, DateTime StartTime, DateTime EndTime, TalentDetailDto[] Talents);

public record TrackDetailDto(Guid Id, string Name, string Description, List<SessionDetailDto> Sessions);

public record TalentDetailDto(Guid Id, string Name, string Type);

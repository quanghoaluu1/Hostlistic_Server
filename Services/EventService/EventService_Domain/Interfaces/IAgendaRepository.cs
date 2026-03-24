using EventService_Domain.Enums;

namespace EventService_Infrastructure.Interfaces;

public interface IAgendaRepository
{
    Task<AgendaQueryResult?> GetAgendaAsync(Guid eventId, Guid? currentUserId);
}

public class AgendaQueryResult
{
    public Guid EventId { get; set; }
    public DateTime? EventStartDate { get; set; }
    public DateTime? EventEndDate { get; set; }
    public List<AgendaTrackData> Tracks { get; set; } = [];
}
 
public class AgendaTrackData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<AgendaSessionData> Sessions { get; set; } = [];
}
 
public class AgendaSessionData
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? TotalCapacity { get; set; }
    public int BookedCount { get; set; }
    public string? VenueName { get; set; }
    public SessionStatus Status { get; set; }
    public bool IsBookedByCurrentUser { get; set; }
    public List<AgendaSpeakerData> Speakers { get; set; } = [];
}
 
public class AgendaSpeakerData
{
    public Guid TalentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

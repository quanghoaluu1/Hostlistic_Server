using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public class SessionDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid TrackId { get; set; }
    public Guid? VenueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? TotalCapacity { get; set; }
    public int BookedCount { get; set; }       // computed
    public SessionStatus Status { get; set; }
    public QaMode? QaMode { get; set; }
    public int SortOrder { get; set; }
    public string? VenueName { get; set; }     // denormalized
    public string? TrackName { get; set; }  
    public string TrackColorHex { get; set; } = string.Empty;
    
    public List<SessionSpeakerDto> Speakers { get; set; } = [];
    
}
public class SessionSpeakerDto
{
    public Guid TalentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Type { get; set; } // "Speaker", "Panelist", etc. from Talent.Type
}
public class CreateSessionRequest
{
    public Guid TrackId { get; set; }
    public Guid? VenueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? TotalCapacity { get; set; }
    public QaMode? QaMode { get; set; }
}

public class UpdateSessionRequest
{
    public Guid? TrackId { get; set; }          // cho phép move session sang track khác
    public Guid? VenueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? TotalCapacity { get; set; }
    public QaMode? QaMode { get; set; }
}
public class UpdateSessionStatusRequest
{
    public SessionStatus Status { get; set; }
}
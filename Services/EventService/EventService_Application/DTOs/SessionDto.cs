using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public class SessionDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid VenueId { get; set; }
    public Guid TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalCapacity { get; set; }
    public SessionStatus Status { get; set; }
    public QaMode? QaMode { get; set; }
}

public class CreateSessionRequest
{
    public Guid EventId { get; set; }
    public Guid? VenueId { get; set; }
    public Guid TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalCapacity { get; set; }
    public QaMode? QaMode { get; set; }
}

public class UpdateSessionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalCapacity { get; set; }
    public SessionStatus Status { get; set; }
    public QaMode QaMode { get; set; }
}
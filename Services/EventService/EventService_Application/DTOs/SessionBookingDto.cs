using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public class SessionBookingResponse
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? TrackName { get; set; }
    public string? TrackColorHex { get; set; }
    public string? VenueName { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime BookingDate { get; set; }
    public List<ConflictWarning>? Warnings { get; set; }  // soft warnings
}

public class ConflictWarning
{
    public Guid ConflictingSessionId { get; set; }
    public string ConflictingSessionTitle { get; set; } = string.Empty;
    public string? TrackName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class MyScheduleResponse
{
    public Guid EventId { get; set; }
    public int TotalBookings { get; set; }
    public List<MyScheduleItemDto> Bookings { get; set; } = [];
}


public class MyScheduleItemDto
{
    public Guid BookingId { get; set; }
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? TrackName { get; set; }
    public string? TrackColorHex { get; set; }
    public string? VenueName { get; set; }
    public SessionStatus SessionStatus { get; set; }
    public DateTime BookingDate { get; set; }
}
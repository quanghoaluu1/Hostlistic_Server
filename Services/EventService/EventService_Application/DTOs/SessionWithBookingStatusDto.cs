namespace EventService_Application.DTOs;

public record SessionWithBookingStatusDto
{
    public required Guid SessionId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public required int SortOrder { get; init; }
    public required string SessionStatus { get; init; }
    public Guid? TrackId { get; init; }
    public required string TrackName { get; init; }
    public required string TrackColorHex { get; init; }
    public Guid? VenueId { get; init; }
    public required string VenueName { get; init; }
    public int? TotalCapacity { get; init; }
    public required int BookedCount { get; init; }
    public int? AvailableSeats { get; init; }
    public required bool IsBooked { get; init; }
    public required bool IsFull { get; init; }
}

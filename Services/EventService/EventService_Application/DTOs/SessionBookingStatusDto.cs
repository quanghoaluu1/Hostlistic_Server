namespace EventService_Application.DTOs;

public record SessionBookingStatusDto
{
    public required Guid SessionId { get; init; }
    public required bool IsBooked { get; init; }
    public Guid? BookingId { get; init; }
    public DateTime? BookingDate { get; init; }
    public required int BookedCount { get; init; }
    public int? AvailableSeats { get; init; }
    public required bool IsFull { get; init; }
}

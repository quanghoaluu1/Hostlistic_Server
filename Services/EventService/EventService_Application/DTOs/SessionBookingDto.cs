using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public class SessionBookingDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; }
}

public class CreateSessionBookingRequest
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateSessionBookingRequest
{
    public BookingStatus Status { get; set; }
}
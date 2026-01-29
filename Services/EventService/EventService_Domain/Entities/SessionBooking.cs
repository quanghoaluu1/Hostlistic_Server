using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class SessionBooking
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; }
}
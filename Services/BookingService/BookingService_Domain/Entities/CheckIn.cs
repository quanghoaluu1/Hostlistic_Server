using BookingService_Domain.Enum;

namespace BookingService_Domain.Entities;

public class CheckIn
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EventId { get; set; }
    public Guid TicketId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid CheckedInByUserId { get; set; }
    public DateTime CheckInTime { get; set; }
    public CheckInType CheckInType { get; set; }

    // ECST denormalized fields for dashboard queries (avoids cross-service joins)
    public string AttendeeName { get; set; } = string.Empty;
    public string AttendeeEmail { get; set; } = string.Empty;
    public string TicketCode { get; set; } = string.Empty;
    public string TicketTypeName { get; set; } = string.Empty;
    public string? SessionName { get; set; }
    public string EventTitle { get; set; } = string.Empty;

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}

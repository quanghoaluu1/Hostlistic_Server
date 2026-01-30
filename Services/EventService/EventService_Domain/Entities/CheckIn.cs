using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class CheckIn
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid EventId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid CheckedBy { get; set; }
    public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;
    public string CheckInLocation { get; set; } = string.Empty;
    public CheckInType CheckInType { get; set; }
    
}
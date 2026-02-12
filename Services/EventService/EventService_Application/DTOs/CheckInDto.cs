using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public class CheckInDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid EventId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid CheckedBy { get; set; }
    public DateTime CheckedInAt { get; set; }
    public string CheckInLocation { get; set; } = string.Empty;
    public CheckInType CheckInType { get; set; }
}

public class CreateCheckInRequest
{
    public Guid TicketId { get; set; }
    public Guid EventId { get; set; }
    public Guid? SessionId { get; set; }
    public string CheckInLocation { get; set; } = string.Empty;
    public CheckInType CheckInType { get; set; }
}

public class UpdateCheckInRequest
{
    public string CheckInLocation { get; set; } = string.Empty;
    public CheckInType CheckInType { get; set; }
}

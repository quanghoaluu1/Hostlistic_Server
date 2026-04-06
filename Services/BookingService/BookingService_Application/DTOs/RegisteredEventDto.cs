namespace BookingService_Application.DTOs;

public class RegisteredEventDto
{
    public Guid EventId { get; set; }
    // Enriched từ EventService
    public string Title { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string EventMode { get; set; } = string.Empty;
    public string EventStatus { get; set; } = string.Empty;

    // Từ BookingService
    public Guid OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public int TotalTickets { get; set; }
    public List<RegisteredEventTicketDto> Tickets { get; set; } = [];
}

public class RegisteredEventTicketDto
{
    public Guid TicketId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string TicketTypeName { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public string? HolderName { get; set; }
}
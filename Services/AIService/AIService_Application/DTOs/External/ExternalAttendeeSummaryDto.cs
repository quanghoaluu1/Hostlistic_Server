namespace AIService_Application.DTOs.External;

/// <summary>
/// Mirrors BookingService AttendeeSummaryDto.
/// Source: GET /api/events/{eventId}/attendees/summary
/// </summary>
public class ExternalAttendeeSummaryDto
{
    public int TotalOrders { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalCheckedIn { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ExternalTicketTypeSummaryDto> ByTicketType { get; set; } = [];
}

public class ExternalTicketTypeSummaryDto
{
    public string TicketTypeName { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int CheckedInCount { get; set; }
    public decimal Revenue { get; set; }
}

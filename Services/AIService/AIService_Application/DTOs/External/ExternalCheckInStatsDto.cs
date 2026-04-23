namespace AIService_Application.DTOs.External;

/// <summary>
/// Mirrors BookingService CheckInStatsResponse.
/// Source: GET /api/checkin/event/{eventId}/stats
/// </summary>
public class ExternalCheckInStatsDto
{
    public int TotalCheckedIn { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalSessionCheckIns { get; set; }
    public List<ExternalTicketTypeCheckInBreakdown> ByTicketType { get; set; } = [];
    public List<ExternalSessionCheckInBreakdown> BySessions { get; set; } = [];
}

public class ExternalTicketTypeCheckInBreakdown
{
    public string TicketTypeName { get; set; } = string.Empty;
    public int CheckedIn { get; set; }
    public int TotalSold { get; set; }
}

public class ExternalSessionCheckInBreakdown
{
    public Guid SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public int CheckedIn { get; set; }
}

namespace BookingService_Application.DTOs;

// Request
public class AttendeeListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public Guid? TicketTypeId { get; set; }
    public bool? IsCheckedIn { get; set; }
    public string SortBy { get; set; } = "orderDate";
    public string SortOrder { get; set; } = "desc";
}

// Individual attendee (one per Order, since one user = one order per purchase)
public class AttendeeDto
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerAvatarUrl { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalTickets { get; set; }
    public int CheckedInTickets { get; set; }
    public List<AttendeeTicketDto> Tickets { get; set; } = [];
}

// Individual ticket within an attendee's order
public class AttendeeTicketDto
{
    public Guid TicketId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string TicketTypeName { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public string? HolderName { get; set; }
    public string? HolderEmail { get; set; }
    public string? HolderPhone { get; set; }
}

// Paged response
public class AttendeeListResponse
{
    public List<AttendeeDto> Attendees { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// Summary stats
public class AttendeeSummaryDto
{
    public int TotalOrders { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalCheckedIn { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<TicketTypeSummaryDto> ByTicketType { get; set; } = [];
}

public class TicketTypeSummaryDto
{
    public string TicketTypeName { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int CheckedInCount { get; set; }
    public decimal Revenue { get; set; }
}

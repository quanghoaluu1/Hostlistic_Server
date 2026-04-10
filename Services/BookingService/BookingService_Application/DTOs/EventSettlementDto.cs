using BookingService_Domain.Enum;

namespace BookingService_Application.DTOs;

public record EventSettlementDto(
    Guid Id,
    Guid EventId,
    Guid OrganizerId,
    decimal GrossVenue,
    decimal PlatformFeePercent,
    decimal PlatformFeeAmount,
    decimal NetRevenue,
    int TotalTicketsSold,
    int TotalOrders,
    SettlementStatus Status,
    string? AdminNotes,
    string? RejectionReason,
    Guid? SettledByAdminId,
    DateTime? SettledAt
    );
public record SettlementPreviewDto(
    Guid EventId,
    string EventTitle,
    Guid OrganizerId,
    decimal GrossRevenue,
    decimal PlatformFeePercent,
    decimal PlatformFeeAmount,
    decimal NetAmount,
    int TotalOrders,
    int TotalTicketsSold,
    bool AlreadySettled
);

public record SettleEventRequest(Guid OrganizerId, string? AdminNotes);

public record RejectSettlementRequest(string Reason);
public class UnsettledEventDto
{
    public Guid EventId { get; set; }
    public decimal GrossRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalTicketsSold { get; set; }
}
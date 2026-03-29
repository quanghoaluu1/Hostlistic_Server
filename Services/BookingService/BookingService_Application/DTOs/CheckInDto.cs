using BookingService_Domain.Enum;

namespace BookingService_Application.DTOs;

public record CheckInScanRequest(
    string QrPayload,
    Guid EventId,
    Guid? SessionId);  // null = event-level check-in

public record CheckInScanResponse(
    Guid CheckInId,
    string TicketCode,
    string AttendeeName,
    string TicketTypeName,
    string? SessionName,
    DateTime CheckInTime,
    bool IsSessionCheckIn);

public record CheckInDto(
    Guid Id,
    Guid TicketId,
    Guid? SessionId,
    string TicketCode,
    string AttendeeName,
    string AttendeeEmail,
    string TicketTypeName,
    string? SessionName,
    string EventTitle,
    DateTime CheckInTime,
    CheckInType CheckInType,
    Guid CheckedInByUserId);

public record TicketTypeBreakdown(
    string TicketTypeName,
    int CheckedIn,
    int TotalSold);

public record SessionBreakdown(
    Guid SessionId,
    string SessionName,
    int CheckedIn);

public record CheckInStatsResponse(
    int TotalCheckedIn,
    int TotalTicketsSold,
    int TotalSessionCheckIns,
    List<TicketTypeBreakdown> ByTicketType,
    List<SessionBreakdown> BySessions);

public record TicketCheckInStatusResponse(
    bool IsCheckedIn,
    DateTime? EventCheckInTime,
    List<CheckInDto> SessionCheckIns);

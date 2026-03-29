using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface ICheckInService
{
    Task<ApiResponse<CheckInScanResponse>> ScanAsync(
        CheckInScanRequest request,
        Guid staffUserId,
        CancellationToken ct = default);

    Task<ApiResponse<List<CheckInDto>>> GetEventCheckInsAsync(
        Guid eventId,
        CancellationToken ct = default);

    Task<ApiResponse<CheckInStatsResponse>> GetEventCheckInStatsAsync(
        Guid eventId,
        CancellationToken ct = default);

    Task<ApiResponse<TicketCheckInStatusResponse>> GetTicketCheckInStatusAsync(
        Guid eventId,
        Guid ticketId,
        CancellationToken ct = default);
}

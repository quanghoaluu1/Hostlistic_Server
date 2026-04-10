using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface ISettlementService
{
    Task<ApiResponse<EventSettlementDto>> SettleEventAsync(Guid eventId, Guid organizerId, Guid adminId,
        string? notes = null, CancellationToken ct = default);   
    Task<ApiResponse<List<UnsettledEventDto>>>
        GetPendingSettlementsAsync(CancellationToken ct);
    Task<ApiResponse<List<EventSettlementDto>>> GetAllSettlementsAsync(CancellationToken ct = default);

    Task<ApiResponse<SettlementPreviewDto>> PreviewSettlementAsync(Guid eventId, Guid organizerId,
        CancellationToken ct = default);

    Task<ApiResponse<EventSettlementDto>> RejectSettlementAsync(
        Guid eventId, Guid adminId, string reason, CancellationToken ct = default);
}
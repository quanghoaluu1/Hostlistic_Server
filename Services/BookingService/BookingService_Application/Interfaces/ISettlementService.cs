using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface ISettlementService
{
    Task<ApiResponse<EventSettlementDto>> SettleEventAsync(Guid eventId, Guid organizerId);
}
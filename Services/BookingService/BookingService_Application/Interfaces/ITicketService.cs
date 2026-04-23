using Common;
using BookingService_Application.DTOs;
using BookingService_Domain.Enum;

namespace BookingService_Application.Interfaces;

public interface ITicketService
{
    Task<ApiResponse<TicketDto>> GetTicketByIdAsync(Guid ticketId);
    Task<ApiResponse<TicketDto>> GetTicketByCodeAsync(string ticketCode);
    Task<ApiResponse<IEnumerable<TicketDto>>> GetTicketsByOrderIdAsync(Guid orderId);
    Task<ApiResponse<TicketDto>> CreateTicketAsync(CreateTicketRequest request);
    Task<ApiResponse<TicketDto>> UpdateTicketAsync(Guid ticketId, UpdateTicketRequest request);
    Task<ApiResponse<bool>> DeleteTicketAsync(Guid ticketId);
    Task<ApiResponse<int>> RegenerateAllQrCodesAsync();
    Task<ApiResponse<GuestLiveAccessTicketDto>> ValidateGuestLiveAccessAsync(ValidateGuestLiveAccessRequest request);
    Task<ApiResponse<TicketDto>> ProcessPostponementDecisionAsync(Guid ticketId, PostponementDecision decision, Guid callerUserId);
    Task<ApiResponse<int>> ProcessRefundsForPostponedEventAsync(Guid eventId);
}


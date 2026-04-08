using Common;
using BookingService_Application.DTOs;

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
}
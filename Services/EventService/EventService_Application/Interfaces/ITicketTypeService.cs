using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ITicketTypeService
{
    Task<ApiResponse<TicketTypeDto>> GetTicketTypeByIdAsync(Guid ticketTypeId);
    Task<ApiResponse<IEnumerable<TicketTypeDto>>> GetTicketTypesByEventIdAsync(Guid eventId);
    Task<ApiResponse<IEnumerable<TicketTypeDto>>> GetTicketTypesBySessionIdAsync(Guid sessionId);
    Task<ApiResponse<TicketTypeDto>> CreateTicketTypeAsync(CreateTicketTypeRequest request);
    Task<ApiResponse<TicketTypeDto>> UpdateTicketTypeAsync(Guid ticketTypeId, UpdateTicketTypeRequest request);
    Task<ApiResponse<bool>> DeleteTicketTypeAsync(Guid ticketTypeId);
}

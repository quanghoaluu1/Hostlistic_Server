using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ITicketTypeService
{
    Task<ApiResponse<TicketTypeDto>> GetTicketTypeByIdAsync(Guid ticketTypeId);
    Task<ApiResponse<PagedResult<TicketTypeDto>>> GetTicketTypesByEventIdAsync(Guid eventId, BaseQueryParams request);
    Task<ApiResponse<PagedResult<TicketTypeDto>>> GetTicketTypesBySessionIdAsync(Guid sessionId, BaseQueryParams request);
    Task<ApiResponse<TicketTypeDto>> CreateTicketTypeAsync(CreateTicketTypeRequest request);
    Task<ApiResponse<TicketTypeDto>> UpdateTicketTypeAsync(Guid ticketTypeId, UpdateTicketTypeRequest request);
    Task<ApiResponse<bool>> DeleteTicketTypeAsync(Guid ticketTypeId);
    Task<ApiResponse<TicketTypeDto>> ProcessTicketPurchaseAsync(Guid ticketTypeId, int quantity);
    Task<ApiResponse<bool>> ProcessBulkTicketPurchaseAsync(BulkTicketPurchaseRequest request);


}

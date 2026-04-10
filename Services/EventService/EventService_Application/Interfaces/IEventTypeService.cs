using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface IEventTypeService
{
    Task<ApiResponse<EventTypeResponse>> CreateEventTypeAsync(CreateEventTypeDto eventType);
    Task<ApiResponse<PagedResult<EventTypeResponse>>> GetAllEventTypesAsync(BaseQueryParams request);
    Task<ApiResponse<EventTypeResponse>> GetEventTypeByIdAsync(Guid eventTypeId);
    Task<ApiResponse<EventTypeResponse>> UpdateEventTypeAsync(Guid eventTypeId, UpdateEventTypeDto eventType);
}
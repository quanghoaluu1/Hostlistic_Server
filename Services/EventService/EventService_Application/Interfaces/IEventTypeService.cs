using Common;
using EventService_Application.DTOs;
using EventService_Domain;

namespace EventService_Application.Interfaces;

public interface IEventTypeService
{
    Task<ApiResponse<EventTypeResponse>> CreateEventTypeAsync(CreateEventTypeDto eventType);
    Task<ApiResponse<IReadOnlyList<EventTypeDto>>> GetAllEventTypesAsync();
    Task<ApiResponse<EventTypeDto>> GetEventTypeByIdAsync(Guid eventTypeId);
    Task<ApiResponse<EventTypeDto>> UpdateEventTypeAsync(Guid eventTypeId, UpdateEventTypeDto eventType);
}
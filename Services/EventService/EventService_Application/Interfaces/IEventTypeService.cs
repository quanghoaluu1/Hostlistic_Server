using Common;
using EventService_Application.DTOs;
using EventService_Domain;

namespace EventService_Application.Interfaces;

public interface IEventTypeService
{
    Task<ApiResponse<EventTypeResponse>> CreateEventTypeAsync(CreateEventTypeDto eventType);
    Task<ApiResponse<IReadOnlyList<EventTypeResponse>>> GetAllEventTypesAsync();
    Task<ApiResponse<EventTypeResponse>> GetEventTypeByIdAsync(Guid eventTypeId);
    Task<ApiResponse<EventTypeResponse>> UpdateEventTypeAsync(Guid eventTypeId, UpdateEventTypeDto eventType);
}
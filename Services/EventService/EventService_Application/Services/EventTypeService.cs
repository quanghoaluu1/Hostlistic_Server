using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class EventTypeService(IEventTypeRepository eventTypeRepository) : IEventTypeService
{
    public async Task<ApiResponse<EventTypeResponse>> CreateEventTypeAsync(CreateEventTypeDto request)
    {
        if (request.Name.IsWhiteSpace())
            return new ApiResponse<EventTypeResponse>()
            {
                IsSuccess = false,
                Message = "Name is required",
                Data = null
            };
        var result = request.Adapt<EventType>();
        result.Id = Guid.NewGuid();
        result.IsActive = true;
        eventTypeRepository.AddEventTypeAsync(result);
        await eventTypeRepository.SaveChangesAsync();
        return ApiResponse<EventTypeResponse>.Success(201, "Event type created successfully", result.Adapt<EventTypeResponse>());
    }

    public async Task<ApiResponse<PagedResult<EventTypeResponse>>> GetAllEventTypesAsync(BaseQueryParams request)
    {
        var eventTypes = await eventTypeRepository.GetAllEventTypesAsync(request);
        var result = eventTypes.Adapt<List<EventTypeResponse>>();
        var pagedResult = new PagedResult<EventTypeResponse>(result, eventTypes.TotalItems, eventTypes.CurrentPage, eventTypes.PageSize);
        return ApiResponse<PagedResult<EventTypeResponse>>.Success(200, "Event types retrieved successfully", pagedResult);
    }

    public async Task<ApiResponse<EventTypeResponse>> GetEventTypeByIdAsync(Guid eventTypeId)
    {
        var eventType = await eventTypeRepository.GetEventTypeByIdAsync(eventTypeId);
        var result = eventType.Adapt<EventTypeResponse>();
        return result == null ? ApiResponse<EventTypeResponse>.Fail(404, "Event type not found") : ApiResponse<EventTypeResponse>.Success(200, "Event type retrieved successfully", result);
    }

    public async Task<ApiResponse<EventTypeResponse>> UpdateEventTypeAsync(Guid eventTypeId,
        UpdateEventTypeDto eventTypeToUpdate)
    {
        var existedEventType = await eventTypeRepository.GetEventTypeByIdAsync(eventTypeId);
        if (existedEventType == null)
            return ApiResponse<EventTypeResponse>.Fail(404, "Event type not found");
        existedEventType.Name = eventTypeToUpdate.Name ?? existedEventType.Name;
        existedEventType.IsActive = eventTypeToUpdate.IsActive ?? existedEventType.IsActive;
        eventTypeRepository.UpdateEventTypeAsync(existedEventType);
        await eventTypeRepository.SaveChangesAsync();
        var result = existedEventType.Adapt<EventTypeResponse>();
        return ApiResponse<EventTypeResponse>.Success(200, "Event type updated successfully", result);
    }
}
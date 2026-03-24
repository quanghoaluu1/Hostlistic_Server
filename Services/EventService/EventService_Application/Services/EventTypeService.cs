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

    public async Task<ApiResponse<IReadOnlyList<EventTypeResponse>>> GetAllEventTypesAsync()
    {
        var eventTypes = await eventTypeRepository.GetAllEventTypesAsync();
        var result = eventTypes.Adapt<IReadOnlyList<EventTypeResponse>>();
        return ApiResponse<IReadOnlyList<EventTypeResponse>>.Success(200, "Event types retrieved successfully", result);
    }

    public async Task<ApiResponse<PagedResult<EventTypeResponse>>> GetAllEventTypesAsync(EventTypeRequest? request)
    {
        var pagedEventTypes = await eventTypeRepository.GetAllEventTypesAsync(request.Name, request.Page, request.PageSize, request.SortBy);
        var eventTypeDtos = pagedEventTypes.Items.Adapt<List<EventTypeResponse>>();
        var result = new PagedResult<EventTypeResponse>
        (
             eventTypeDtos,
             pagedEventTypes.TotalItems,
             pagedEventTypes.CurrentPage,
             pagedEventTypes.PageSize
        );

        return ApiResponse<PagedResult<EventTypeResponse>>.Success(200, "Event types retrieved successfully", result);
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
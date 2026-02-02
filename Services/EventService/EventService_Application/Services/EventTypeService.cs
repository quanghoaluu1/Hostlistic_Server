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

    public async Task<ApiResponse<IReadOnlyList<EventTypeDto>>> GetAllEventTypesAsync()
    {
        var eventTypes =  await eventTypeRepository.GetAllEventTypesAsync();
        var result = eventTypes.Adapt<IReadOnlyList<EventTypeDto>>();
        return ApiResponse<IReadOnlyList<EventTypeDto>>.Success(200, "Event types retrieved successfully", result);
    }

    public async Task<ApiResponse<EventTypeDto>> GetEventTypeByIdAsync(Guid eventTypeId)
    {
        var eventType = await eventTypeRepository.GetEventTypeByIdAsync(eventTypeId);
        var result = eventType.Adapt<EventTypeDto>();
        return result == null ? ApiResponse<EventTypeDto>.Fail(404, "Event type not found") : ApiResponse<EventTypeDto>.Success(200, "Event type retrieved successfully", result);
    }
    
    public async Task<ApiResponse<EventTypeDto>> UpdateEventTypeAsync(Guid eventTypeId,
        UpdateEventTypeDto eventTypeToUpdate)
    {
        var existedEventType = await eventTypeRepository.GetEventTypeByIdAsync(eventTypeId);
        if (existedEventType == null)
            return ApiResponse<EventTypeDto>.Fail(404, "Event type not found");
        existedEventType.Name = eventTypeToUpdate.EventType.Name ?? existedEventType.Name;       
        existedEventType.IsActive = eventTypeToUpdate.EventType.IsActive ?? existedEventType.IsActive;
        eventTypeRepository.UpdateEventTypeAsync(existedEventType);
        await eventTypeRepository.SaveChangesAsync();
        var result = existedEventType.Adapt<EventTypeDto>();
        return ApiResponse<EventTypeDto>.Success(200, "Event type updated successfully", result);
    }
}
using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface IEventService
{
    Task<ApiResponse<EventResponseDto>> CreateEventAsync(EventRequestDto request);
    Task<ApiResponse<IReadOnlyCollection<EventResponseDto>>> GetAllEventsAsync();
    Task<ApiResponse<EventResponseDto>> GetEventByIdAsync(Guid eventId);
    Task<ApiResponse<EventResponseDto>> UpdateEventAsync(Guid eventId, EventRequestDto request, string? publicId);
}
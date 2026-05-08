using Common;
using EventService_Application.DTOs;
using EventService_Domain.Models;

namespace EventService_Application.Interfaces;

public interface IEventService
{
    Task<ApiResponse<EventResponseDto>> CreateEventAsync(
        EventRequestDto request,
        Guid organizerId,
        string? organizerFullName = null,
        string? organizerEmail = null);
    Task<ApiResponse<PagedResult<EventResponseDto>>> GetAllEventsAsync(AdminEventQueryParams request);
    Task<ApiResponse<EventResponseDto>> GetEventByIdAsync(Guid eventId);
    Task<ApiResponse<EventResponseDto>> UpdateEventAsync(Guid eventId, EventRequestDto request, string? publicId);
    Task<ApiResponse<PagedResult<MyEventDto>>> GetMyEventAsync(Guid userId, MyEventQueryParams queryParams);
    Task<ApiResponse<PagedResult<PublicEventDto>>> GetPublicEventsAsync(PublicEventQueryParams queryParams);
    Task<ApiResponse<bool>> ToggleAgendaModeAsync(Guid eventId);
    Task<ApiResponse<StreamAuthResponseDto>> VerifyStreamAccessAsync(Guid eventId, Guid userId, Guid? trackId = null);
    Task<ApiResponse<object>> GetEventDashboardAsync(int? year, int? month);
    Task<ApiResponse<bool>> UpdateEventStatus(Guid eventId);
    Task<ApiResponse<PagedResult<EventResponseDto>>> GetPostponedEventsAsync(BaseQueryParams request);
}

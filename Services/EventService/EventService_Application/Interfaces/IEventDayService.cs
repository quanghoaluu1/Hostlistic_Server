using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface IEventDayService
{
    Task<ApiResponse<IReadOnlyList<EventDayResponse>>> GetByEventIdAsync(Guid eventId);
    Task<ApiResponse<EventDayResponse>> GetByIdAsync(Guid eventId, Guid dayId);
    Task<ApiResponse<IReadOnlyList<EventDayResponse>>> GenerateDaysAsync(Guid eventId, GenerateEventDaysRequest request);
    Task<ApiResponse<EventDayResponse>> CreateAsync(Guid eventId, CreateEventDayRequest request);
    Task<ApiResponse<EventDayResponse>> UpdateAsync(Guid eventId, Guid dayId, UpdateEventDayRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid eventId, Guid dayId);
}

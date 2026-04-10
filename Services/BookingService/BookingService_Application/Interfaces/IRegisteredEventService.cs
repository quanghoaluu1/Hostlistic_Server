using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Services;

public interface IRegisteredEventService
{
    Task<ApiResponse<List<RegisteredEventDto>>> GetMyRegisteredEventsAsync(Guid userId);
}
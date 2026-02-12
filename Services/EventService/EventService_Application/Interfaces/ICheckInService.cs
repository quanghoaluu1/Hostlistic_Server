using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ICheckInService
{
    Task<ApiResponse<CheckInDto>> GetCheckInByIdAsync(Guid checkInId);
    Task<ApiResponse<IEnumerable<CheckInDto>>> GetCheckInsByEventIdAsync(Guid eventId);
    Task<ApiResponse<IEnumerable<CheckInDto>>> GetCheckInsBySessionIdAsync(Guid sessionId);
    Task<ApiResponse<CheckInDto?>> GetCheckInByTicketIdAsync(Guid ticketId);
    Task<ApiResponse<CheckInDto>> CreateCheckInAsync(Guid checkedByUserId, CreateCheckInRequest request);
    Task<ApiResponse<CheckInDto>> UpdateCheckInAsync(Guid checkInId, UpdateCheckInRequest request);
    Task<ApiResponse<bool>> DeleteCheckInAsync(Guid checkInId);
}

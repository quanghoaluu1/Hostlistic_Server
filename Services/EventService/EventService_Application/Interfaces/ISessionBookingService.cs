using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISessionBookingService
{
    Task<ApiResponse<SessionBookingDto>> GetSessionBookingByIdAsync(Guid bookingId);
    Task<ApiResponse<PagedResult<SessionBookingDto>>> GetSessionBookingsByUserIdAsync(Guid userId, BaseQueryParams request);
    Task<ApiResponse<PagedResult<SessionBookingDto>>> GetSessionBookingsBySessionIdAsync(Guid sessionId, BaseQueryParams request);
    Task<ApiResponse<SessionBookingDto>> CreateSessionBookingAsync(CreateSessionBookingRequest request);
    Task<ApiResponse<SessionBookingDto>> UpdateSessionBookingAsync(Guid bookingId, UpdateSessionBookingRequest request);
    Task<ApiResponse<bool>> DeleteSessionBookingAsync(Guid bookingId);
}
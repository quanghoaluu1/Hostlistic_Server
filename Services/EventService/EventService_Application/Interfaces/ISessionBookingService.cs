using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISessionBookingService
{
    Task<ApiResponse<SessionBookingDto>> GetSessionBookingByIdAsync(Guid bookingId);
    Task<ApiResponse<IEnumerable<SessionBookingDto>>> GetSessionBookingsBySessionIdAsync(Guid sessionId);
    Task<ApiResponse<IEnumerable<SessionBookingDto>>> GetSessionBookingsByUserIdAsync(Guid userId);
    Task<ApiResponse<SessionBookingDto>> CreateSessionBookingAsync(CreateSessionBookingRequest request);
    Task<ApiResponse<SessionBookingDto>> UpdateSessionBookingAsync(Guid bookingId, UpdateSessionBookingRequest request);
    Task<ApiResponse<bool>> DeleteSessionBookingAsync(Guid bookingId);
}
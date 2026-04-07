using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISessionBookingService
{
    // Task<ApiResponse<SessionBookingDto>> GetSessionBookingByIdAsync(Guid bookingId);
    // Task<ApiResponse<IEnumerable<SessionBookingDto>>> GetSessionBookingsBySessionIdAsync(Guid sessionId);
    // Task<ApiResponse<IEnumerable<SessionBookingDto>>> GetSessionBookingsByUserIdAsync(Guid userId);
    // Task<ApiResponse<SessionBookingDto>> CreateSessionBookingAsync(CreateSessionBookingRequest request);
    // Task<ApiResponse<SessionBookingDto>> UpdateSessionBookingAsync(Guid bookingId, UpdateSessionBookingRequest request);
    // Task<ApiResponse<bool>> DeleteSessionBookingAsync(Guid bookingId);
    /// <summary>
    /// Book a session for the authenticated user.
    /// Validates: session exists, not full, not already booked, session is bookable.
    /// Returns soft warnings if user has overlapping booked sessions.
    /// </summary>
    Task<ApiResponse<SessionBookingResponse>> BookSessionAsync(Guid eventId, Guid sessionId, Guid userId);
 
    /// <summary>
    /// Cancel a session booking. Only the booking owner can cancel.
    /// </summary>
    Task<ApiResponse<bool>> CancelBookingAsync(Guid eventId, Guid sessionId, Guid userId);
 
    /// <summary>
    /// Get all confirmed session bookings for the authenticated user within an event.
    /// Used for the "My Schedule" view in the attendee portal.
    /// </summary>
    Task<ApiResponse<MyScheduleResponse>> GetMyScheduleAsync(Guid eventId, Guid userId);
}
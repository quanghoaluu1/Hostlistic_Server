using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISessionBookingService
{
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

    /// <summary>
    /// Get all sessions for an event with the authenticated user's booking status embedded.
    /// Allows the attendee portal to display the full session list with IsBooked/AvailableSeats
    /// in a single API call, avoiding a client-side merge of two separate requests.
    /// </summary>
    Task<ApiResponse<List<SessionWithBookingStatusDto>>> GetSessionsWithBookingStatusAsync(Guid eventId, Guid userId);

    /// <summary>
    /// Get the booking status for a single session.
    /// Used when rendering the Book/Cancel button in session detail pages.
    /// </summary>
    Task<ApiResponse<SessionBookingStatusDto>> GetBookingStatusForSessionAsync(Guid eventId, Guid sessionId, Guid userId);
}

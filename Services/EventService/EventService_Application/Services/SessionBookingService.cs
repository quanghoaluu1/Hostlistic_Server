using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;

namespace EventService_Application.Services;

public class SessionBookingService(
    ISessionBookingRepository bookingRepository,
    ISessionRepository sessionRepository) : ISessionBookingService
{
    // ═══════════════════════════════════════════════════════════════════════
    //  BOOK SESSION
    //
    //  Validation chain:
    //    1. Session exists + belongs to event
    //    2. Session is in bookable state (Scheduled or OnGoing)
    //    3. User hasn't already booked this session
    //    4. Capacity check (if session has a cap)
    //    5. Conflict detection → SOFT WARNING (booking proceeds)
    //
    //  The soft warning pattern:
    //    Response is 201 Created (success), but the body includes a
    //    Warnings[] array listing sessions that overlap in time.
    //    Frontend displays these as non-blocking toast notifications.
    //    The user is informed but not blocked — they may intentionally
    //    want to attend partial sessions across tracks.
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<SessionBookingResponse>> BookSessionAsync(
        Guid eventId, Guid sessionId, Guid userId)
    {
        // ── Guard 1: Session exists + belongs to event ──
        var session = await sessionRepository.GetByIdWithinEventAsync(eventId, sessionId);
        if (session is null)
            return ApiResponse<SessionBookingResponse>.Fail(404, "Session not found in this event");

        // ── Guard 2: Session must be bookable ──
        if (session.Status is SessionStatus.Cancelled)
            return ApiResponse<SessionBookingResponse>.Fail(400,
                "Cannot book a cancelled session");

        if (session.Status is SessionStatus.Completed)
            return ApiResponse<SessionBookingResponse>.Fail(400,
                "Cannot book a completed session");

        // ── Guard 3: Not already booked ──
        var hasActiveBooking = await bookingRepository.UserHasBookingForSessionAsync(userId, sessionId);
        if (hasActiveBooking)
            return ApiResponse<SessionBookingResponse>.Fail(409,
                "You have already booked this session");

        // ── Guard 4: Capacity check ──
        if (session.TotalCapacity.HasValue)
        {
            var bookedCount = await sessionRepository.GetBookedCountAsync(sessionId);
            if (bookedCount >= session.TotalCapacity.Value)
                return ApiResponse<SessionBookingResponse>.Fail(409,
                    "Session is full — no available seats");
        }

        // ── Step 5: Conflict detection — SOFT WARNING ──
        // Find overlapping sessions this user has already booked
        List<ConflictWarning>? warnings = null;

        if (session.StartTime.HasValue && session.EndTime.HasValue)
        {
            var conflictingSessions = await bookingRepository.GetConflictingSessionsAsync(
                userId, eventId,
                session.StartTime.Value,
                session.EndTime.Value,
                excludeSessionId: sessionId);

            if (conflictingSessions.Count > 0)
            {
                warnings = conflictingSessions.Select(c => new ConflictWarning
                {
                    ConflictingSessionId = c.Id,
                    ConflictingSessionTitle = c.Title,
                    TrackName = c.Track?.Name,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime
                }).ToList();
                // NOTE: We DO NOT return here — booking proceeds with warning
            }
        }

        // ── Create booking ──
        var booking = new SessionBooking
        {
            Id = Guid.CreateVersion7(),
            SessionId = sessionId,
            UserId = userId,
            Status = BookingStatus.Confirmed,
            BookingDate = DateTime.UtcNow
        };

        await bookingRepository.AddSessionBookingAsync(booking);
        await bookingRepository.SaveChangesAsync();

        // ── Build response ──
        var response = new SessionBookingResponse
        {
            Id = booking.Id,
            SessionId = sessionId,
            SessionTitle = session.Title,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            TrackName = session.Track?.Name,
            TrackColorHex = session.Track?.ColorHex,
            VenueName = session.Venue?.Name,
            Status = BookingStatus.Confirmed,
            BookingDate = booking.BookingDate,
            Warnings = warnings
        };

        return ApiResponse<SessionBookingResponse>.Success(201, "Session booked", response);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CANCEL BOOKING
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<bool>> CancelBookingAsync(
        Guid eventId, Guid sessionId, Guid userId)
    {
        // Find the user's booking for this session
        var booking = await bookingRepository.GetByUserAndSessionAsync(userId, sessionId);

        if (booking is null)
            return ApiResponse<bool>.Fail(404, "No booking found for this session");

        // Verify it belongs to the correct event
        if (booking.Session.EventId != eventId)
            return ApiResponse<bool>.Fail(404, "Booking not found in this event");

        if (booking.Status != BookingStatus.Confirmed)
            return ApiResponse<bool>.Fail(400,
                $"Cannot cancel a {booking.Status} booking");

        // Soft delete — change status, don't remove record
        // Preserves audit trail and prevents unique constraint issues on re-booking
        booking.Status = BookingStatus.Cancelled;
        await bookingRepository.UpdateSessionBookingAsync(booking);
        await bookingRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Booking cancelled", true);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  MY SCHEDULE
    //
    //  Returns all confirmed session bookings for the user within an event.
    //  Used for the "My Schedule" tab in the attendee portal.
    //  Sorted by session start time — reads like a personal agenda.
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<MyScheduleResponse>> GetMyScheduleAsync(
        Guid eventId, Guid userId)
    {
        var bookings = await bookingRepository.GetByUserAndEventAsync(userId, eventId);

        var items = bookings.Select(b => new MyScheduleItemDto
        {
            BookingId = b.Id,
            SessionId = b.SessionId,
            SessionTitle = b.Session.Title,
            StartTime = b.Session.StartTime,
            EndTime = b.Session.EndTime,
            TrackName = b.Session.Track?.Name,
            TrackColorHex = b.Session.Track?.ColorHex,
            VenueName = b.Session.Venue?.Name,
            SessionStatus = b.Session.Status,
            BookingDate = b.BookingDate
        }).ToList();

        var response = new MyScheduleResponse
        {
            EventId = eventId,
            TotalBookings = items.Count,
            Bookings = items
        };

        return ApiResponse<MyScheduleResponse>.Success(200, "Schedule retrieved", response);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GET SESSIONS WITH BOOKING STATUS
    //
    //  Returns all sessions for an event with the current user's booking
    //  status embedded in each item (IsBooked, AvailableSeats, IsFull).
    //  Avoids requiring the frontend to make two separate API calls and
    //  merge the data client-side.
    //
    //  NOTE: This method makes N+1 calls for GetBookedCountAsync (one per
    //  session). This is acceptable for thesis scope given typical session
    //  counts per event. A future optimization would be to add a batch method
    //  GetBookedCountsAsync(IEnumerable<Guid> sessionIds) that returns a
    //  Dictionary<Guid, int> from a single GROUP BY query, eliminating the
    //  N+1 pattern entirely.
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<List<SessionWithBookingStatusDto>>> GetSessionsWithBookingStatusAsync(
        Guid eventId, Guid userId)
    {
        // Single round trip: all sessions for this event (includes Track + Venue nav props)
        var sessions = await sessionRepository.GetSessionsByEventIdAsync(eventId);

        // Single round trip: set of session IDs the user has confirmed bookings for
        var bookedIds = await bookingRepository.GetBookedSessionIdsAsync(userId, eventId);

        var result = new List<SessionWithBookingStatusDto>();

        foreach (var session in sessions)
        {
            var bookedCount = await sessionRepository.GetBookedCountAsync(session.Id);

            int? availableSeats = session.TotalCapacity.HasValue
                ? Math.Max(0, session.TotalCapacity.Value - bookedCount)
                : null;

            result.Add(new SessionWithBookingStatusDto
            {
                SessionId = session.Id,
                Title = session.Title,
                Description = session.Description,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                SortOrder = session.SortOrder,
                SessionStatus = session.Status.ToString(),
                TrackId = session.TrackId,
                TrackName = session.Track?.Name ?? string.Empty,
                TrackColorHex = session.Track?.ColorHex ?? string.Empty,
                VenueId = session.VenueId,
                VenueName = session.Venue?.Name ?? string.Empty,
                TotalCapacity = session.TotalCapacity,
                BookedCount = bookedCount,
                AvailableSeats = availableSeats,
                IsBooked = bookedIds.Contains(session.Id),
                IsFull = session.TotalCapacity.HasValue && bookedCount >= session.TotalCapacity.Value
            });
        }

        var sorted = result
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.SortOrder)
            .ToList();

        return ApiResponse<List<SessionWithBookingStatusDto>>.Success(200, "Sessions retrieved", sorted);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GET BOOKING STATUS FOR SESSION
    //
    //  Returns the booking status for a single session.
    //  Used when rendering the Book/Cancel button in session detail pages.
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<SessionBookingStatusDto>> GetBookingStatusForSessionAsync(
        Guid eventId, Guid sessionId, Guid userId)
    {
        // Verify session exists in this event
        var session = await sessionRepository.GetByIdWithinEventAsync(eventId, sessionId);
        if (session is null)
            return ApiResponse<SessionBookingStatusDto>.Fail(404, "Session not found in this event");

        // Get the user's booking for this session (may be null)
        var booking = await bookingRepository.GetByUserAndSessionAsync(userId, sessionId);

        var bookedCount = await sessionRepository.GetBookedCountAsync(sessionId);

        int? availableSeats = session.TotalCapacity.HasValue
            ? Math.Max(0, session.TotalCapacity.Value - bookedCount)
            : null;

        var dto = new SessionBookingStatusDto
        {
            SessionId = sessionId,
            IsBooked = booking is not null && booking.Status == BookingStatus.Confirmed,
            BookingId = booking != null ? (Guid?)booking.Id : null,
            BookingDate = booking != null ? (DateTime?)booking.BookingDate : null,
            BookedCount = bookedCount,
            AvailableSeats = availableSeats,
            IsFull = session.TotalCapacity.HasValue && bookedCount >= session.TotalCapacity.Value
        };

        return ApiResponse<SessionBookingStatusDto>.Success(200, "Booking status retrieved", dto);
    }
}

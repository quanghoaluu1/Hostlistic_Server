using Common;
using Common.Messages;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;
using MassTransit;

namespace EventService_Application.Services;

public class SessionService(
    ITrackRepository trackRepository,
    IEventRepository eventRepository,
    ISessionRepository sessionRepository,
    IPublishEndpoint publishEndpoint) : ISessionService
{


    public async Task<ApiResponse<SessionDto>> GetSessionByIdAsync(Guid eventId, Guid sessionId)
    {
        var session = await sessionRepository.GetByIdWithinEventAsync(eventId, sessionId);
        if (session is null)
            return ApiResponse<SessionDto>.Fail(404, "Session not found");

        var sessionDto = await MapToDtoAsync(session);
        return ApiResponse<SessionDto>.Success(200, "Session retrieved successfully", sessionDto);
    }

    public async Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsByEventIdAsync(Guid eventId)
    {
        var eventExists = await eventRepository.EventExistsAsync(eventId);
        if (!eventExists)
            return ApiResponse<IEnumerable<SessionDto>>.Fail(404, "Event not found");
        var sessions = await sessionRepository.GetSessionsByEventIdAsync(eventId);
        var sessionDtos = new List<SessionDto>();
        foreach (var session in sessions)
            sessionDtos.Add(await MapToDtoAsync(session));
        return ApiResponse<IEnumerable<SessionDto>>.Success(200, "Sessions retrieved successfully", sessionDtos);
    }

    public async Task<ApiResponse<PagedResult<SessionDto>>> GetSessionsByTrackIdAsync(Guid eventId, Guid trackId, BaseQueryParams request)
    {
        var trackExists = await trackRepository.ExistsWithinEventAsync(eventId, trackId);
        if (!trackExists)
            return ApiResponse<PagedResult<SessionDto>>.Fail(404, "Track not found in this event");

        var sessions = await sessionRepository.GetSessionsByTrackIdAsync(trackId, request);
        var sessionDtos = sessions.Adapt<List<SessionDto>>();
        var result = new PagedResult<SessionDto>
            (
                sessionDtos,
                sessions.TotalItems,
                sessions.CurrentPage,
                sessions.PageSize
            );
        //    = new List<SessionDto>();
        //foreach (var session in sessions)
        //    sessionDtos.Add(await MapToDtoAsync(session));

        return ApiResponse<PagedResult<SessionDto>>.Success(200, "Sessions retrieved", result);
    }

    public async Task<ApiResponse<PagedResult<SessionDto>>> GetSessionsByVenueIdAsync(Guid venueId, BaseQueryParams request)
    {
        var sessions = await sessionRepository.GetSessionsByVenueIdAsync(venueId, request);
        var sessionDtos = sessions.Adapt<List<SessionDto>>();
        var result = new PagedResult<SessionDto>
            (
                sessionDtos,
                sessions.TotalItems,
                sessions.CurrentPage,
                sessions.PageSize
            );
        return ApiResponse<PagedResult<SessionDto>>.Success(200, "Sessions retrieved successfully", result);
    }

    public async Task<ApiResponse<SessionDto>> CreateSessionAsync(
    Guid eventId, CreateSessionRequest request)
    {
        // ── Guard 1: Basic time sanity (no DB call needed) ──
        if (request.StartTime >= request.EndTime)
            return ApiResponse<SessionDto>.Fail(400, "Start time must be before end time");

        var duration = request.EndTime - request.StartTime;


        if (string.IsNullOrWhiteSpace(request.Title))
            return ApiResponse<SessionDto>.Fail(400, "Session title is required");

        // ── Guard 2: Track exists AND belongs to this event ──
        // Single query: GetByIdWithinEventAsync loads Track + Event navigation
        var track = await trackRepository.GetByIdWithinEventAsync(eventId, request.TrackId);
        if (track is null)
            return ApiResponse<SessionDto>.Fail(404,
                "Track not found in this event");

        // ── Guard 3: Session time within event date range ──
        var start = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);
        var eventEntity = track.Event;

        if (eventEntity.StartDate.HasValue && start < eventEntity.StartDate.Value)
            return ApiResponse<SessionDto>.Fail(400,
                "Session cannot start before event start date");

        if (eventEntity.EndDate.HasValue && end > eventEntity.EndDate.Value)
            return ApiResponse<SessionDto>.Fail(400,
                "Session cannot end after event end date");

        // ── Guard 4: Track overlap — HARD BLOCK ──
        // Design decision: 1 track = 1 sequential content stream.
        // Two sessions in the same track cannot run in parallel.
        //
        // Interval overlap theorem: A overlaps B iff A.Start < B.End AND A.End > B.Start
        // Implemented at SQL level via EXISTS — O(log n) with the composite index.
        var hasTrackOverlap = await sessionRepository
            .HasOverlapInTrackAsync(request.TrackId, start, end);

        if (hasTrackOverlap)
            return ApiResponse<SessionDto>.Fail(409,
                "Time conflict: another session in this track overlaps with the given time range");

        // ── Guard 5: Venue overlap — HARD BLOCK (only if venue specified) ──
        // Physical constraint: one room cannot host two sessions simultaneously.
        if (request.VenueId.HasValue)
        {
            var hasVenueOverlap = await sessionRepository
                .HasOverlapInVenueAsync(request.VenueId.Value, start, end);

            if (hasVenueOverlap)
                return ApiResponse<SessionDto>.Fail(409,
                    "Venue conflict: another session at this venue overlaps with the given time range");
        }
 
        eventEntity.PromoteToCustomAgenda();
        // ── All guards passed — create entity ──
        var maxSort = await sessionRepository.GetMaxSortOrderInTrackAsync(request.TrackId);

        var session = new Session
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            TrackId = request.TrackId,
            VenueId = request.VenueId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            StartTime = start,
            EndTime = end,
            TotalCapacity = request.TotalCapacity,
            Status = SessionStatus.Scheduled,
            QaMode = request.QaMode,
            SortOrder = maxSort + 1
        };

        await sessionRepository.AddSessionAsync(session);
        await sessionRepository.SaveChangesAsync();

        await publishEndpoint.Publish(new SessionSyncedEvent(
            SessionId: session.Id,
            EventId: session.EventId,
            TrackId: session.TrackId,
            Title: session.Title,
            StartTime: session.StartTime!.Value,
            EndTime: session.EndTime!.Value,
            Location: null,
            SessionOrder: session.SortOrder));

        // Re-fetch to get navigation properties for DTO mapping
        var created = await sessionRepository.GetByIdWithinEventAsync(eventId, session.Id);
        var dto = await MapToDtoAsync(created!);

        return ApiResponse<SessionDto>.Success(201, "Session created", dto);
    }

    public async Task<ApiResponse<SessionDto>> UpdateSessionAsync(
    Guid eventId, Guid sessionId, UpdateSessionRequest request)
    {
        var session = await sessionRepository.GetByIdWithinEventAsync(eventId, sessionId);
        if (session is null)
            return ApiResponse<SessionDto>.Fail(404, "Session not found in this event");

        // Cannot edit a cancelled or completed session's content
        if (session.Status is SessionStatus.Cancelled or SessionStatus.Completed)
            return ApiResponse<SessionDto>.Fail(400,
                $"Cannot edit a {session.Status} session");

        // ── Time validation ──
        if (request.StartTime >= request.EndTime)
            return ApiResponse<SessionDto>.Fail(400, "Start time must be before end time");

        var duration = request.EndTime - request.StartTime;

        if (string.IsNullOrWhiteSpace(request.Title))
            return ApiResponse<SessionDto>.Fail(400, "Session title is required");

        var start = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);

        // ── Event bounds check ──
        var eventEntity = session.Event;
        if (eventEntity.StartDate.HasValue && start < eventEntity.StartDate.Value)
            return ApiResponse<SessionDto>.Fail(400,
                "Session cannot start before event start date");

        if (eventEntity.EndDate.HasValue && end > eventEntity.EndDate.Value)
            return ApiResponse<SessionDto>.Fail(400,
                "Session cannot end after event end date");
        // ── Handle track change (move session to different track) ──
        var targetTrackId = request.TrackId ?? session.TrackId;
        if (request.TrackId.HasValue && request.TrackId.Value != session.TrackId)
        {
            var targetTrack = await trackRepository.GetByIdWithinEventAsync(eventId, request.TrackId.Value);
            if (targetTrack is null)
                return ApiResponse<SessionDto>.Fail(404, "Target track not found in this event");
        }

        // ── Track overlap (exclude self) ──
        var hasTrackOverlap = await sessionRepository
            .HasOverlapInTrackAsync(targetTrackId, start, end, excludeSessionId: sessionId);

        if (hasTrackOverlap)
            return ApiResponse<SessionDto>.Fail(409,
                "Time conflict: another session in this track overlaps with the given time range");

        // ── Venue overlap (exclude self) ──
        if (request.VenueId.HasValue)
        {
            var hasVenueOverlap = await sessionRepository
                .HasOverlapInVenueAsync(request.VenueId.Value, start, end, excludeSessionId: sessionId);

            if (hasVenueOverlap)
                return ApiResponse<SessionDto>.Fail(409,
                    "Venue conflict: another session at this venue overlaps with the given time range");
        }
        eventEntity.PromoteToCustomAgenda();
        // ── Apply changes ──
        session.TrackId = targetTrackId;
        session.VenueId = request.VenueId;
        session.Title = request.Title.Trim();
        session.Description = request.Description?.Trim() ?? string.Empty;
        session.StartTime = start;
        session.EndTime = end;
        session.TotalCapacity = request.TotalCapacity;
        session.QaMode = request.QaMode;

        await sessionRepository.UpdateSessionAsync(session);
        await sessionRepository.SaveChangesAsync();

        await publishEndpoint.Publish(new SessionSyncedEvent(
            SessionId: session.Id,
            EventId: session.EventId,
            TrackId: session.TrackId,
            Title: session.Title,
            StartTime: session.StartTime!.Value,
            EndTime: session.EndTime!.Value,
            Location: null,
            SessionOrder: session.SortOrder));

        var updated = await sessionRepository.GetByIdWithinEventAsync(eventId, sessionId);
        var dto = await MapToDtoAsync(updated!);

        return ApiResponse<SessionDto>.Success(200, "Session updated", dto);
    }

    public async Task<ApiResponse<bool>> DeleteSessionAsync(Guid eventId, Guid sessionId)
    {
        var session = await sessionRepository.GetByIdWithinEventAsync(eventId, sessionId);
        if (session is null)
            return ApiResponse<bool>.Fail(404, "Session not found in this event");

        // Guard: cannot delete session with confirmed bookings
        if (await sessionRepository.HasBookingsAsync(sessionId))
            return ApiResponse<bool>.Fail(409,
                "Cannot delete session with active bookings. Cancel all bookings first.");

        await sessionRepository.DeleteSessionAsync(sessionId);
        await sessionRepository.SaveChangesAsync();

        await publishEndpoint.Publish(new SessionDeletedEvent(
            SessionId: sessionId,
            EventId: eventId));

        return ApiResponse<bool>.Success(200, "Session deleted", true);
    }
    public async Task<ApiResponse<SessionDto>> UpdateSessionStatusAsync(
        Guid eventId, Guid sessionId, UpdateSessionStatusRequest request)
    {
        var session = await sessionRepository.GetByIdWithinEventAsync(eventId, sessionId);
        if (session is null)
            return ApiResponse<SessionDto>.Fail(404, "Session not found in this event");

        var currentStatus = session.Status;
        var newStatus = request.Status;

        if (!IsValidTransition(currentStatus, newStatus))
            return ApiResponse<SessionDto>.Fail(400,
                $"Invalid status transition: {currentStatus} → {newStatus}");

        session.Status = newStatus;

        await sessionRepository.UpdateSessionAsync(session);
        await sessionRepository.SaveChangesAsync();

        var dto = await MapToDtoAsync(session);
        return ApiResponse<SessionDto>.Success(200,
            $"Session status updated to {newStatus}", dto);
    }

    private static bool IsValidTransition(SessionStatus from, SessionStatus to) => (from, to) switch
    {
        (SessionStatus.Scheduled, SessionStatus.OnGoing) => true,
        (SessionStatus.Scheduled, SessionStatus.Cancelled) => true,
        (SessionStatus.OnGoing, SessionStatus.Completed) => true,
        (SessionStatus.OnGoing, SessionStatus.Cancelled) => true,
        _ => false
    };

    private async Task<SessionDto> MapToDtoAsync(Session session)
    {
        var dto = session.Adapt<SessionDto>();

        // Denormalized fields
        dto.VenueName = session.Venue?.Name;
        dto.TrackName = session.Track?.Name ?? string.Empty;
        dto.TrackColorHex = session.Track?.ColorHex ?? string.Empty;

        // Computed booking count
        dto.BookedCount = await sessionRepository.GetBookedCountAsync(session.Id);

        // Speakers from Lineup
        dto.Speakers = session.Lineups?
            .Where(l => l.Talent != null)
            .Select(l => new SessionSpeakerDto
            {
                TalentId = l.Talent.Id,
                Name = l.Talent.Name,
                AvatarUrl = l.Talent.AvatarUrl,
                Type = l.Talent.Type
            }).ToList() ?? [];

        return dto;
    }
}
using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<ApiResponse<SessionDto>> GetSessionByIdAsync(Guid sessionId)
    {
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
            return ApiResponse<SessionDto>.Fail(404, "Session not found");

        var sessionDto = session.Adapt<SessionDto>();
        return ApiResponse<SessionDto>.Success(200, "Session retrieved successfully", sessionDto);
    }

    public async Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsByEventIdAsync(Guid eventId)
    {
        var sessions = await _sessionRepository.GetSessionsByEventIdAsync(eventId);
        var sessionDtos = sessions.Adapt<IEnumerable<SessionDto>>();
        return ApiResponse<IEnumerable<SessionDto>>.Success(200, "Sessions retrieved successfully", sessionDtos);
    }

    public async Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsByTrackIdAsync(Guid trackId)
    {
        var sessions = await _sessionRepository.GetSessionsByTrackIdAsync(trackId);
        var sessionDtos = sessions.Adapt<IEnumerable<SessionDto>>();
        return ApiResponse<IEnumerable<SessionDto>>.Success(200, "Sessions retrieved successfully", sessionDtos);
    }

    public async Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsByVenueIdAsync(Guid venueId)
    {
        var sessions = await _sessionRepository.GetSessionsByVenueIdAsync(venueId);
        var sessionDtos = sessions.Adapt<IEnumerable<SessionDto>>();
        return ApiResponse<IEnumerable<SessionDto>>.Success(200, "Sessions retrieved successfully", sessionDtos);
    }

    public async Task<ApiResponse<SessionDto>> CreateSessionAsync(CreateSessionRequest request)
    {
        if (request.StartTime >= request.EndTime)
            return ApiResponse<SessionDto>.Fail(400, "Start time must be before end time");

        var session = request.Adapt<Session>();
        session.Status = EventService_Domain.Enums.SessionStatus.Scheduled;

        await _sessionRepository.AddSessionAsync(session);
        await _sessionRepository.SaveChangesAsync();

        var sessionDto = session.Adapt<SessionDto>();
        return ApiResponse<SessionDto>.Success(201, "Session created successfully", sessionDto);
    }

    public async Task<ApiResponse<SessionDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request)
    {
        var existingSession = await _sessionRepository.GetSessionByIdAsync(sessionId);
        if (existingSession == null)
            return ApiResponse<SessionDto>.Fail(404, "Session not found");

        if (request.StartTime >= request.EndTime)
            return ApiResponse<SessionDto>.Fail(400, "Start time must be before end time");

        // Update properties
        existingSession.Title = request.Title;
        existingSession.Description = request.Description;
        existingSession.StartTime = request.StartTime;
        existingSession.EndTime = request.EndTime;
        existingSession.TotalCapacity = request.TotalCapacity;
        existingSession.Status = request.Status;
        existingSession.QaMode = request.QaMode;

        await _sessionRepository.UpdateSessionAsync(existingSession);
        await _sessionRepository.SaveChangesAsync();

        var sessionDto = existingSession.Adapt<SessionDto>();
        return ApiResponse<SessionDto>.Success(200, "Session updated successfully", sessionDto);
    }

    public async Task<ApiResponse<bool>> DeleteSessionAsync(Guid sessionId)
    {
        var exists = await _sessionRepository.SessionExistsAsync(sessionId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Session not found");

        var deleted = await _sessionRepository.DeleteSessionAsync(sessionId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete session");

        await _sessionRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Session deleted successfully", true);
    }
}
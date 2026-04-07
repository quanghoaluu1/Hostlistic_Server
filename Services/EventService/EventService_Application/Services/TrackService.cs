using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class TrackService : ITrackService
{
    private readonly ITrackRepository _trackRepository;
    private readonly IEventRepository _eventRepository;

    public TrackService(ITrackRepository trackRepository, IEventRepository eventRepository)
    {
        _trackRepository = trackRepository;
        _eventRepository = eventRepository;
    }

    public async Task<ApiResponse<TrackDto>> GetTrackByIdAsync(Guid eventId, Guid trackId)
    {
        var track = await _trackRepository.GetByIdWithinEventAsync(eventId, trackId);
        if (track == null)
            return ApiResponse<TrackDto>.Fail(404, "Track not found");

        return ApiResponse<TrackDto>.Success(200, "Track retrieved successfully", MapToDto(track));
    }

    public async Task<ApiResponse<PagedResult<TrackDto>>> GetTracksByEventIdAsync(Guid eventId, BaseQueryParams request)
    {
        var tracks = await _trackRepository.GetTracksByEventIdAsync(eventId, request);
        var trackDtos = tracks.Adapt<List<TrackDto>>();
        var result = new PagedResult<TrackDto>
        (
            trackDtos,
            tracks.TotalItems,
            tracks.CurrentPage,
            tracks.PageSize
        );
        return ApiResponse<PagedResult<TrackDto>>.Success(200, "Tracks retrieved successfully", result);
    }

    public async Task<ApiResponse<TrackDto>> CreateTrackAsync(Guid eventId, CreateTrackRequest request)
    {
        var eventEntity = await _eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity is null)
            return ApiResponse<TrackDto>.Fail(404, "Event not found");
        
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<TrackDto>.Fail(400, "Track name is required");

        if (request.StartTime.HasValue && request.EndTime.HasValue)
        {
            if (request.StartTime.Value >= request.EndTime.Value)
                return ApiResponse<TrackDto>.Fail(400, "Track start time must be before end time");
 
            if (eventEntity.StartDate.HasValue && request.StartTime.Value < eventEntity.StartDate.Value)
                return ApiResponse<TrackDto>.Fail(400,
                    "Track start time cannot be before event start date");
 
            if (eventEntity.EndDate.HasValue && request.EndTime.Value > eventEntity.EndDate.Value)
                return ApiResponse<TrackDto>.Fail(400,
                    "Track end time cannot be after event end date");
        }

        if (!IsValidHexColor(request.ColorHex))
            return ApiResponse<TrackDto>.Fail(400, "Invalid color format. Use hex like #6366F1");
        
        var maxSort = await _trackRepository.GetMaxSortOrderAsync(eventId);
        var track = new Track()
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            StartTime = NormalizeToUtc(request.StartTime),
            EndTime = NormalizeToUtc(request.EndTime),
            ColorHex = request.ColorHex,
            SortOrder = maxSort + 1
        };

        await _trackRepository.AddTrackAsync(track);
        await _trackRepository.SaveChangesAsync();

        return ApiResponse<TrackDto>.Success(201, "Track created successfully", MapToDto(track));
    }

    public async Task<ApiResponse<TrackDto>> UpdateTrackAsync(Guid eventId, Guid trackId, UpdateTrackRequest request)
    {
        var track = await _trackRepository.GetByIdWithinEventAsync(eventId, trackId);
        if (track is null)
            return ApiResponse<TrackDto>.Fail(404, "Track not found in this event");
 
        // ── Validate name ──
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<TrackDto>.Fail(400, "Track name is required");
 
        // ── Validate time range ──
        if (request.StartTime.HasValue && request.EndTime.HasValue)
        {
            if (request.StartTime.Value >= request.EndTime.Value)
                return ApiResponse<TrackDto>.Fail(400, "Start time must be before end time");
 
            var eventEntity = track.Event;
            if (eventEntity.StartDate.HasValue && request.StartTime.Value < eventEntity.StartDate.Value)
                return ApiResponse<TrackDto>.Fail(400,
                    "Track start time cannot be before event start date");
 
            if (eventEntity.EndDate.HasValue && request.EndTime.Value > eventEntity.EndDate.Value)
                return ApiResponse<TrackDto>.Fail(400,
                    "Track end time cannot be after event end date");
        }
 
        if (!IsValidHexColor(request.ColorHex))
            return ApiResponse<TrackDto>.Fail(400, "Invalid color format");
 
        // ── Apply changes ──
        track.Name = request.Name.Trim();
        track.Description = request.Description?.Trim();
        track.StartTime = NormalizeToUtc(request.StartTime);
        track.EndTime = NormalizeToUtc(request.EndTime);
        track.ColorHex = request.ColorHex;
 
        if (request.SortOrder.HasValue)
            track.SortOrder = request.SortOrder.Value;
 
        await _trackRepository.UpdateTrackAsync(track);
        await _trackRepository.SaveChangesAsync();
 
        return ApiResponse<TrackDto>.Success(200, "Track updated", MapToDto(track));
    }

    public async Task<ApiResponse<bool>> DeleteTrackAsync(Guid eventId, Guid trackId)
    {
        var track = await _trackRepository.GetByIdWithinEventAsync(eventId, trackId);
        if (track is null)
            return ApiResponse<bool>.Fail(404, "Track not found in this event");
 
        // ── Guard: cannot delete track with sessions ──
        // DeleteBehavior.Restrict on FK would throw DB exception.
        // We catch it here with a clear business error instead.
        if (await _trackRepository.HasSessionsAsync(trackId))
            return ApiResponse<bool>.Fail(409,
                "Cannot delete track with existing sessions. Remove all sessions first.");
 
        await _trackRepository.DeleteTrackAsync(trackId);
        await _trackRepository.SaveChangesAsync();
 
        return ApiResponse<bool>.Success(200, "Track deleted", true);
    }
    
    private static TrackDto MapToDto(Track track)
    {
        var dto = track.Adapt<TrackDto>();
        dto.SessionCount = track.Sessions?.Count ?? 0;
        return dto;
    }
 
    private static DateTime? NormalizeToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
 
    private static bool IsValidHexColor(string hex)
        => !string.IsNullOrWhiteSpace(hex)
           && hex.StartsWith('#')
           && (hex.Length == 4 || hex.Length == 7 || hex.Length == 9); // #RGB, #RRGGBB, #RRGGBBAA
}
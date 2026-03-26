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

    public TrackService(ITrackRepository trackRepository)
    {
        _trackRepository = trackRepository;
    }

    public async Task<ApiResponse<TrackDto>> GetTrackByIdAsync(Guid trackId)
    {
        var track = await _trackRepository.GetTrackByIdAsync(trackId);
        if (track == null)
            return ApiResponse<TrackDto>.Fail(404, "Track not found");

        var trackDto = track.Adapt<TrackDto>();
        return ApiResponse<TrackDto>.Success(200, "Track retrieved successfully", trackDto);
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

    public async Task<ApiResponse<TrackDto>> CreateTrackAsync(CreateTrackRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<TrackDto>.Fail(400, "Track name is required");

        if (string.IsNullOrWhiteSpace(request.ColorHex))
            return ApiResponse<TrackDto>.Fail(400, "Track color is required");

        var track = request.Adapt<Track>();

        await _trackRepository.AddTrackAsync(track);
        await _trackRepository.SaveChangesAsync();

        var trackDto = track.Adapt<TrackDto>();
        return ApiResponse<TrackDto>.Success(201, "Track created successfully", trackDto);
    }

    public async Task<ApiResponse<TrackDto>> UpdateTrackAsync(Guid trackId, UpdateTrackRequest request)
    {
        var existingTrack = await _trackRepository.GetTrackByIdAsync(trackId);
        if (existingTrack == null)
            return ApiResponse<TrackDto>.Fail(404, "Track not found");

        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<TrackDto>.Fail(400, "Track name is required");

        if (string.IsNullOrWhiteSpace(request.ColorHex))
            return ApiResponse<TrackDto>.Fail(400, "Track color is required");

        // Update properties
        existingTrack.Name = request.Name;
        existingTrack.Description = request.Description;
        existingTrack.ColorHex = request.ColorHex;

        await _trackRepository.UpdateTrackAsync(existingTrack);
        await _trackRepository.SaveChangesAsync();

        var trackDto = existingTrack.Adapt<TrackDto>();
        return ApiResponse<TrackDto>.Success(200, "Track updated successfully", trackDto);
    }

    public async Task<ApiResponse<bool>> DeleteTrackAsync(Guid trackId)
    {
        var exists = await _trackRepository.TrackExistsAsync(trackId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Track not found");

        var deleted = await _trackRepository.DeleteTrackAsync(trackId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete track");

        await _trackRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Track deleted successfully", true);
    }
}
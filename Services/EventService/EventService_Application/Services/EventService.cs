using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class EventService(IEventRepository eventRepository, ITrackService trackService, ISessionService sessionService) : IEventService
{
    public async Task<ApiResponse<EventResponseDto>> CreateEventAsync(EventRequestDto request)
    {
        var eventEntity = request.Adapt<Event>();
        eventEntity.StartDate = request.StartDate == null 
            ? null 
            : DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc);
        eventEntity.EndDate = request.EndDate == null 
            ? null 
            : DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc);
        eventEntity.EventStatus = EventStatus.Draft;
        eventEntity.IsPublic = false;
        var defaultTrack = new Track
        {
            Name = "Main Track",
            Description = "Main track for the event",
            StartTime = eventEntity.StartDate,
            EndTime = eventEntity.EndDate,
            ColorHex = "#000000",
            Sessions = new List<Session>()
        };

        var defaultSession = new Session
        {
            Title = "Main Session",
            Description = "Main session for the event",
            StartTime = eventEntity.StartDate,
            EndTime = eventEntity.EndDate,
            TotalCapacity = request.TotalCapacity,
            VenueId = eventEntity.VenueId
        };
        defaultTrack.Sessions.Add(defaultSession);
        eventRepository.AddEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        var responseDto = eventEntity.Adapt<EventResponseDto>();
        return ApiResponse<EventResponseDto>.Success(201, "Event created successfully", responseDto);
    }
    public async Task<ApiResponse<IReadOnlyCollection<EventResponseDto>>> GetAllEventsAsync()
    {
        var events = await eventRepository.GetAllEventsAsync();
        var responseDtos = events.Adapt<IReadOnlyCollection<EventResponseDto>>();
        return ApiResponse<IReadOnlyCollection<EventResponseDto>>.Success(200, "Events retrieved successfully", responseDtos);
    }
    public async Task<ApiResponse<EventResponseDto>> GetEventByIdAsync(Guid eventId)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        var responseDto = eventEntity.Adapt<EventResponseDto>();
        return responseDto == null ? ApiResponse<EventResponseDto>.Fail(404, "Event not found") : ApiResponse<EventResponseDto>.Success(200, "Event retrieved successfully", responseDto);
    }

    public async Task<ApiResponse<EventResponseDto>> UpdateEventAsync(Guid eventId, EventRequestDto request, string? publicId)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity == null)
            return ApiResponse<EventResponseDto>.Fail(404, "Event not found");
        ApplyEventUpdate(eventEntity, request);
        eventEntity.CoverImagePublicId = publicId;
        eventRepository.UpdateEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();
        var responseDto = eventEntity.Adapt<EventResponseDto>();
        return ApiResponse<EventResponseDto>.Success(200, "Event updated successfully", responseDto);
    }

    private void ApplyEventUpdate(Event eventEntity, EventRequestDto request)
    {
        eventEntity.Title = request.Title ?? eventEntity.Title;
        eventEntity.Description = request.Description ?? eventEntity.Description;
        eventEntity.Location = request.Location ?? eventEntity.Location;
        eventEntity.CoverImageUrl = request.CoverImageUrl ?? eventEntity.CoverImageUrl;
        
        eventEntity.EventMode = request.EventMode ?? eventEntity.EventMode;
        eventEntity.TotalCapacity = request.TotalCapacity ?? eventEntity.TotalCapacity;
        eventEntity.EventTypeId = request.EventTypeId ?? eventEntity.EventTypeId;
        eventEntity.VenueId = request.VenueId ?? eventEntity.VenueId;
        eventEntity.IsPublic = request.IsPublic ?? eventEntity.IsPublic;
        if (request.StartDate.HasValue)
        {
            eventEntity.StartDate = DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc);
        }

        if (request.EndDate.HasValue)
        {
            eventEntity.EndDate = DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc);
        }
    }
}
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
    public async Task<ApiResponse<EventResponseDto>> CreateEventAsync(CreateEventDto request)
    {
        var eventEntity = request.Adapt<Event>();
        eventEntity.StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        eventEntity.EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);
        eventEntity.EventStatus = EventStatus.Draft;
        eventEntity.IsPublic = false;
        var defaultTrack = new Track
        {
            Name = "Main Track",
            Description = "Main track for the event",
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
}
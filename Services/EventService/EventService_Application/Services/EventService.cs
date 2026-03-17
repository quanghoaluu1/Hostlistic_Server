using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class EventService(
    IEventRepository eventRepository, 
    ITrackService trackService, 
    ISessionService sessionService, 
    IEventTeamMemberRepository eventTeamMemberRepository) : IEventService
{
    public async Task<ApiResponse<EventResponseDto>> CreateEventAsync(EventRequestDto request, Guid organizerId)
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
        eventEntity.Id = Guid.NewGuid();
        eventEntity.OrganizerId = organizerId;

        var defaultTrack = new Track
        {
            Id = Guid.NewGuid(),
            Name = "Main Track",
            Description = "Main track for the event",
            StartTime = eventEntity.StartDate,
            EndTime = eventEntity.EndDate,
            ColorHex = "#000000",
            Sessions = new List<Session>(),
            EventId = eventEntity.Id
        };

        var defaultSession = new Session
        {
            Id = Guid.NewGuid(),
            Title = "Main Session",
            Description = "Main session for the event",
            StartTime = eventEntity.StartDate,
            EndTime = eventEntity.EndDate,
            TotalCapacity = request.TotalCapacity,
            VenueId = eventEntity.VenueId,
            EventId = eventEntity.Id
        };
        defaultTrack.Sessions.Add(defaultSession);
        eventEntity.Tracks.Add(defaultTrack);

        var organizerMember = new EventTeamMember
        {
            Id = Guid.NewGuid(),
            UserId = organizerId,
            EventId = eventEntity.Id,
            Role = EventRole.Organizer,
            Status = EventMemberStatus.Active,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTime.UtcNow,
            Permissions = new Dictionary<string, bool>
            {
                { "can_checkin", true },
                { "can_edit_event", true },
                { "can_manage_members", true }
            }
        };
        eventEntity.EventTeamMembers.Add(organizerMember);

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

    public async Task<ApiResponse<PagedResult<MyEventDto>>> GetMyEventAsync(Guid userId, MyEventQueryParams p)
    {
        var orgQuery = eventRepository.GetQueryable()
            .Where(e => e.OrganizerId == userId);

        if (p.Status.HasValue)
            orgQuery = orgQuery.Where(e => e.EventStatus.Equals(p.Status.Value));

        if (!string.IsNullOrWhiteSpace(p.Search))
            orgQuery = orgQuery.Where(e => e.Title.Contains(p.Search));

        var asOrganizer = p.Role.HasValue && p.Role.Value != EventRole.Organizer
            ? new List<MyEventDto>()
            : orgQuery.Select(e => new MyEventDto(
                e.Id, e.Title, e.CoverImageUrl,
                e.StartDate.Value, e.EndDate.Value,
                e.EventMode.ToString(),
                (int)e.EventStatus, e.Location,
                nameof(EventRole.Organizer),
                e.CreatedAt
            )).ToList();

        var memberQuery = eventTeamMemberRepository.GetQueryableByUserId(userId)
            .Where(m => m.Role != EventRole.Organizer);

        if (p.Status.HasValue)
            memberQuery = memberQuery.Where(m => m.Event.EventStatus.Equals(p.Status.Value));

        if (!string.IsNullOrWhiteSpace(p.Search))
            memberQuery = memberQuery.Where(m => m.Event.Title.Contains(p.Search));

        if (p.Role.HasValue && p.Role.Value != EventRole.Organizer)
            memberQuery = memberQuery.Where(m => m.Role == p.Role.Value);

        var asTeamMember = p.Role.HasValue && p.Role.Value == EventRole.Organizer
            ? new List<MyEventDto>()
            : memberQuery.Select(m => new MyEventDto(
                m.Event.Id, m.Event.Title, m.Event.CoverImageUrl,
                m.Event.StartDate.Value, m.Event.EndDate.Value,
                m.Event.EventMode.ToString(),
                (int)m.Event.EventStatus, m.Event.Location,
                m.Role.ToString(),
                m.JoinedAt ?? m.InvitedAt
            )).ToList();

        var allEvents = asOrganizer.Concat(asTeamMember)
            .OrderByDescending(e => e.StartDate)
            .ToList();

        var totalCount = allEvents.Count;
        var items = allEvents
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToList();

        var result = new PagedResult<MyEventDto>(items, totalCount, p.Page, p.PageSize);
        return ApiResponse<PagedResult<MyEventDto>>.Success(200, "Success", result);
    }

    private void ApplyEventUpdate(Event eventEntity, EventRequestDto request)
    {
        eventEntity.Title = request.Title ?? eventEntity.Title;
        eventEntity.Description = request.Description ?? eventEntity.Description;
        eventEntity.Location = request.Location ?? eventEntity.Location;
        eventEntity.CoverImageUrl = request.CoverImageUrl ?? eventEntity.CoverImageUrl;
        eventEntity.EventStatus = request.EventStatus ?? eventEntity.EventStatus;
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
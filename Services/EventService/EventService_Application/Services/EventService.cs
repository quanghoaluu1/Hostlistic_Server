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

    public async Task<ApiResponse<PagedResult<MyEventDto>>> GetMyEventAsync(
    Guid userId,
    MyEventQueryParams queryParams)
    {
        // Project to an intermediate anonymous type using only DB-translatable expressions.
        // Enum .ToString() causes client-side evaluation which breaks Concat (SQL UNION).
        var organizerQuery = eventRepository.GetQueryable()
            .Where(e => e.OrganizerId == userId)
            .Select(e => new
            {
                EventId = e.Id,
                e.Title,
                e.CoverImageUrl,
                StartDate = e.StartDate.Value,
                EndDate = e.EndDate.Value,
                EventModeValue = e.EventMode,
                Status = e.EventStatus,
                e.Location,
                RoleValue = (int)EventRole.Organizer,
                JoinedAt = e.CreatedAt
            });

        var memberQuery = eventTeamMemberRepository.GetQueryableByUserId(userId)
            .Where(m => m.Role != EventRole.Organizer)
            .Select(m => new
            {
                EventId = m.Event.Id,
                m.Event.Title,
                CoverImageUrl = m.Event.CoverImageUrl,
                StartDate = m.Event.StartDate.Value,
                EndDate = m.Event.EndDate.Value,
                EventModeValue = m.Event.EventMode,
                Status = m.Event.EventStatus,
                m.Event.Location,
                RoleValue = (int)m.Role,
                JoinedAt = m.JoinedAt ?? m.InvitedAt
            });

        var query = organizerQuery.Concat(memberQuery);

        // FILTER
        if (queryParams.Role.HasValue)
        {
            var roleValue = (int)queryParams.Role.Value;
            query = query.Where(e => e.RoleValue == roleValue);
        }

        if (queryParams.Status is not null)
        {
            query = query.Where(e => e.Status.Equals(queryParams.Status));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            query = query.Where(e =>
                e.Title.Contains(queryParams.Search) ||
                e.Location.Contains(queryParams.Search));
        }

        // SORT
        query = query.ApplySorting(queryParams.SortBy);

        // PAGING
        var paged = await query.ToPagedResultAsync(
            queryParams.Page,
            queryParams.PageSize);

        // Map to MyEventDto after materialization (enum .ToString() is safe in memory)
        var items = paged.Items.Select(e => new MyEventDto(
            e.EventId,
            e.Title,
            e.CoverImageUrl,
            e.StartDate,
            e.EndDate,
            ((EventMode)e.EventModeValue).ToString(),
            e.Status.ToString(),
            e.Location,
            ((EventRole)e.RoleValue).ToString(),
            e.JoinedAt
        )).ToList();

        var result = new PagedResult<MyEventDto>(items, paged.TotalItems, paged.CurrentPage, paged.PageSize);

        return ApiResponse<PagedResult<MyEventDto>>
            .Success(200, "Success", result);
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
using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Constants;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

namespace EventService_Application.Services;

public class EventService(
    IEventRepository eventRepository,
    ITrackService trackService,
    ISessionService sessionService,
    IEventTeamMemberRepository eventTeamMemberRepository,
    IUserPlanServiceClient userPlanServiceClient,
    IEventDayService eventDayService,
    ILogger<EventService> logger) : IEventService
{
    public async Task<ApiResponse<EventResponseDto>> CreateEventAsync(
        EventRequestDto request,
        Guid organizerId,
        string? organizerFullName = null,
        string? organizerEmail = null)
    {
        var entitlement = await GetActiveEntitlementAsync(organizerId);
        if (!entitlement.IsSuccess)
            return ApiResponse<EventResponseDto>.Fail(403, entitlement.Message);

        var currentEventCount = eventRepository.GetQueryable()
            .Count(e => e.OrganizerId == organizerId && e.EventStatus != EventStatus.Cancelled && e.EventStatus != EventStatus.Completed );
        if (currentEventCount >= entitlement.MaxEvents)
        {
            return ApiResponse<EventResponseDto>.Fail(403,
                $"Plan limit reached: max events = {entitlement.MaxEvents}");
        }

        if (request.TotalCapacity.HasValue && request.TotalCapacity.Value > entitlement.MaxAttendeesPerEvent)
        {
            return ApiResponse<EventResponseDto>.Fail(403,
                $"Plan limit reached: max attendees per event = {entitlement.MaxAttendeesPerEvent}");
        }

        if (request.StartDate is { } startDate)
        {
            var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            if (startUtc < DateTime.UtcNow)
                return ApiResponse<EventResponseDto>.Fail(400, "Start date cannot be in the past");
        }

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
        eventEntity.TimeZoneId = request.TimeZoneId;
        eventEntity.AgendaMode = AgendaMode.Auto;

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
            Permissions = EventPermissions.AllKeys.ToDictionary(key => key, _ => true),
            UserFullName = organizerFullName,
            UserEmail = organizerEmail
        };
        eventEntity.EventTeamMembers.Add(organizerMember);

        eventRepository.AddEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        // Auto-generate EventDays when the event spans more than one local calendar day.
        // Failure is non-fatal — the organizer can trigger generation manually via POST /days/generate.
        if (eventEntity.StartDate.HasValue && eventEntity.EndDate.HasValue)
        {
            try
            {
                var tzId = eventEntity.TimeZoneId;
                var tz = string.IsNullOrWhiteSpace(tzId)
                    ? TimeZoneInfo.Utc
                    : TimeZoneInfo.FindSystemTimeZoneById(tzId);

                var localStart = DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(eventEntity.StartDate.Value, tz));
                var localEnd = DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(eventEntity.EndDate.Value, tz));

                if (localEnd > localStart)
                {
                    var genResult = await eventDayService.GenerateDaysAsync(
                        eventEntity.Id,
                        new GenerateEventDaysRequest { TimeZoneId = tzId, Force = false });

                    if (!genResult.IsSuccess)
                        logger.LogWarning(
                            "Auto-generation of event days failed for event {EventId}: {Message}",
                            eventEntity.Id, genResult.Message);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Auto-generation of event days threw an unexpected exception for event {EventId}.",
                    eventEntity.Id);
            }
        }

        var responseDto = eventEntity.Adapt<EventResponseDto>();
        return ApiResponse<EventResponseDto>.Success(201, "Event created successfully", responseDto);
    }
    public async Task<ApiResponse<PagedResult<EventResponseDto>>> GetAllEventsAsync(BaseQueryParams request)
    {
        var events = await eventRepository.GetAllEventsAsync(request);
        var responseDtos = events.Items.Adapt<List<EventResponseDto>>();
        var result = new PagedResult<EventResponseDto>(responseDtos, events.TotalItems, events.CurrentPage, events.PageSize);
        return ApiResponse<PagedResult<EventResponseDto>>.Success(200, "Events retrieved successfully", result);
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

        var entitlement = await GetActiveEntitlementAsync(eventEntity.OrganizerId);
        if (!entitlement.IsSuccess)
            return ApiResponse<EventResponseDto>.Fail(403, entitlement.Message);

        if (request.TotalCapacity.HasValue && request.TotalCapacity.Value > entitlement.MaxAttendeesPerEvent)
        {
            return ApiResponse<EventResponseDto>.Fail(403,
                $"Plan limit reached: max attendees per event = {entitlement.MaxAttendeesPerEvent}");
        }

        // Detect whether StartDate or EndDate is changing so we know whether to sync EventDays.
        var incomingStart = request.StartDate.HasValue
            ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;
        var incomingEnd = request.EndDate.HasValue
            ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        bool datesChanged =
            (incomingStart.HasValue && incomingStart != eventEntity.StartDate) ||
            (incomingEnd.HasValue && incomingEnd != eventEntity.EndDate);

        if (datesChanged)
        {
            var startForSync = incomingStart ?? eventEntity.StartDate;
            var endForSync = incomingEnd ?? eventEntity.EndDate;
            var tzIdForSync = !string.IsNullOrWhiteSpace(request.TimeZoneId)
                ? request.TimeZoneId
                : eventEntity.TimeZoneId;

            if (startForSync.HasValue && endForSync.HasValue)
            {
                // Transactional strategy: dry-run sync (read-only conflict check) before persisting
                // event date changes. This prevents the event's dates from ever being committed in a
                // state that contradicts its EventDays without requiring an explicit database transaction
                // (which the rest of this service does not use). The negligible TOCTOU window is
                // acceptable for an organizer-driven admin flow.
                var syncResult = await eventDayService.SyncDaysAsync(
                    eventId, startForSync.Value, endForSync.Value, tzIdForSync);

                if (!syncResult.IsSuccess)
                    return ApiResponse<EventResponseDto>.Fail(syncResult.StatusCode, syncResult.Message);
            }
        }

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
                RoleValue = EventRole.Organizer,
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
                RoleValue = m.Role,
                JoinedAt = m.JoinedAt ?? m.InvitedAt
            });

        var query = organizerQuery.Concat(memberQuery);

        // FILTER
        if (queryParams.Role.HasValue)
        {
            var roleValue = queryParams.Role.Value;
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

    public async Task<ApiResponse<PagedResult<PublicEventDto>>> GetPublicEventsAsync(
    PublicEventQueryParams queryParams)
    {
        // Start from AsNoTracking queryable (defined in repository)
        var query = eventRepository.GetQueryable()
            .Where(e => e.IsPublic == true
                      && e.EventStatus != EventStatus.Draft);

        // FILTER: search by title or location
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var search = queryParams.Search.Trim().ToLower();
            query = query.Where(e =>
                e.Title!.ToLower().Contains(search) ||
                e.Location!.ToLower().Contains(search));
        }

        // FILTER: event mode
        if (queryParams.EventMode.HasValue)
        {
            query = query.Where(e => e.EventMode == queryParams.EventMode.Value);
        }

        // FILTER: event type
        if (queryParams.EventTypeId.HasValue)
        {
            query = query.Where(e => e.EventTypeId == queryParams.EventTypeId.Value);
        }

        // FILTER: specific status (default shows all non-Draft)
        if (queryParams.Status.HasValue)
        {
            query = query.Where(e => e.EventStatus == queryParams.Status.Value);
        }

        // PROJECT to lightweight DTO before sorting/paging
        // This ensures SQL only selects needed columns + single JOIN to EventType
        var projected = query.Select(e => new
        {
            e.Id,
            e.Title,
            e.CoverImageUrl,
            e.StartDate,
            e.EndDate,
            e.Location,
            EventModeValue = e.EventMode,
            StatusValue = e.EventStatus,
            EventTypeName = e.EventType != null ? e.EventType.Name : null,
            e.TotalCapacity,
            e.IsPublic
        });

        // SORT
        projected = projected.ApplySorting(queryParams.SortBy);

        // PAGING
        var paged = await projected.ToPagedResultAsync(
            queryParams.Page,
            queryParams.PageSize);

        // Map to DTO after materialization (enum .ToString() safe in memory)
        var items = paged.Items.Select(e => new PublicEventDto(
            e.Id,
            e.Title ?? string.Empty,
            e.CoverImageUrl,
            e.StartDate,
            e.EndDate,
            e.Location,
            e.EventModeValue.HasValue
                ? e.EventModeValue.Value.ToString()
                : "Offline",
            e.StatusValue.ToString(),
            e.EventTypeName,
            e.TotalCapacity,
            e.IsPublic ?? false
        )).ToList();

        var result = new PagedResult<PublicEventDto>(
            items, paged.TotalItems, paged.CurrentPage, paged.PageSize);

        return ApiResponse<PagedResult<PublicEventDto>>
            .Success(200, "Public events retrieved successfully", result);
    }

    public async Task<ApiResponse<bool>> ToggleAgendaModeAsync(Guid eventId)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity == null)
            return ApiResponse<bool>.Fail(404, "Event not found");
        eventEntity.AgendaMode = eventEntity.AgendaMode == AgendaMode.Auto ? AgendaMode.Custom : AgendaMode.Auto;
        eventRepository.UpdateEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Agenda mode toggled successfully", eventEntity.AgendaMode == AgendaMode.Auto);

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
        eventEntity.IsPublic = request.IsPublic ?? eventEntity.IsPublic;
        if (request.StartDate.HasValue)
        {
            eventEntity.StartDate = DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc);
        }

        if (request.EndDate.HasValue)
        {
            eventEntity.EndDate = DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc);
        }

        if (request.TimeZoneId != null)
            eventEntity.TimeZoneId = request.TimeZoneId;
    }

    private async Task<(bool IsSuccess, string Message, int MaxEvents, int MaxAttendeesPerEvent)> GetActiveEntitlementAsync(
        Guid organizerId)
    {
        var lookup = await userPlanServiceClient.GetByUserIdAsync(organizerId, true);
        if (!lookup.IsSuccess)
        {
            return (false, $"Failed to fetch active subscription plan: {lookup.Message}", 0, 0);
        }

        var activePlans = lookup.Plans;
        var activePlan = activePlans
            .Where(x => x.SubscriptionPlan is not null)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefault();

        if (activePlan?.SubscriptionPlan is null)
        {
            return (false, "No active subscription plan found for this organizer.", 0, 0);
        }

        return (true, "OK", activePlan.SubscriptionPlan.MaxEvents, activePlan.SubscriptionPlan.MaxAttendeesPerEvent);
    }
    public async Task<ApiResponse<StreamAuthResponseDto>> VerifyStreamAccessAsync(Guid eventId, Guid userId)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity == null)
            return ApiResponse<StreamAuthResponseDto>.Fail(404, "Event not found");

        if (eventEntity.EventMode == EventMode.Offline)
            return ApiResponse<StreamAuthResponseDto>.Fail(400, "This is an offline event and does not support streaming.");

        // Check constraints: Allow opening 30 mins before StartDate
        if (eventEntity.StartDate.HasValue)
        {
            var earliestOpenTime = eventEntity.StartDate.Value.AddMinutes(-30);
            if (DateTime.UtcNow < earliestOpenTime)
            {
                return ApiResponse<StreamAuthResponseDto>.Success(200, "Too early to join stream", new StreamAuthResponseDto
                {
                    IsAllowed = false,
                    Role = "None",
                    ErrorMessage = "Phòng Live chỉ được mở trước 30 phút so với thời gian bắt đầu sự kiện."
                });
            }
        }

        // Determine User Role
        string role = "Attendee";
        if (eventEntity.OrganizerId == userId)
        {
            role = "Organizer";
        }
        else
        {
            // Note: Since IQueryable might not support async without EF Core using, we evaluate synchronously or rely on existing repository methods if available.
            // Using synchronous FirstOrDefault for safe IQueryable materialization if EF Core using is missing.
            var teamMembers = eventTeamMemberRepository.GetQueryableByUserId(userId).Where(m => m.EventId == eventId).ToList();
            var member = teamMembers.FirstOrDefault(m => m.Status == EventMemberStatus.Active);

            if (member != null)
            {
                role = member.Role.ToString();
            }
            else
            {
                // In a complete implementation, check EventRegistration/Ticket module here
                role = "Attendee";
            }
        }

        return ApiResponse<StreamAuthResponseDto>.Success(200, "Success", new StreamAuthResponseDto
        {
            IsAllowed = true,
            Role = role,
            ErrorMessage = null
        });
    }

    public async Task<ApiResponse<object>> GetEventDashboardAsync(int? year, int? month)
    {
        var dashboardData = await eventRepository.GetDashboardAsync(year, month);

        return ApiResponse<object>.Success(
            200,
            "Dashboard data retrieved successfully",
            dashboardData
        );
    }

    public async Task<ApiResponse<bool>> UpdateEventStatus(Guid eventId)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity == null)
            return ApiResponse<bool>.Fail(404, "Event not found");
        await eventRepository.UpdateEventStatus(eventEntity);
        return ApiResponse<bool>.Success(200, "Event status updated successfully", true);
    }

    public async Task<ApiResponse<PagedResult<EventResponseDto>>> GetPostponedEventsAsync(BaseQueryParams request)
    {
        var query = eventRepository.GetQueryable()
            .Where(e => e.EventStatus == EventStatus.Postponed);

        query = query.ApplySorting(request.SortBy);

        var paged = await query.ToPagedResultAsync(request.Page, request.PageSize);
        var responseDtos = paged.Items.Adapt<List<EventResponseDto>>();
        
        var result = new PagedResult<EventResponseDto>(responseDtos, paged.TotalItems, paged.CurrentPage, paged.PageSize);
        return ApiResponse<PagedResult<EventResponseDto>>.Success(200, "Postponed events retrieved successfully", result);
    }
}
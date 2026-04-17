using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class EventDayService(
    IEventDayRepository eventDayRepository,
    IEventRepository eventRepository,
    ISessionRepository sessionRepository)
    : IEventDayService
{
    public async Task<ApiResponse<IReadOnlyList<EventDayResponse>>> GetByEventIdAsync(Guid eventId)
    {
        var eventExists = await eventRepository.EventExistsAsync(eventId);
        if (!eventExists)
            return ApiResponse<IReadOnlyList<EventDayResponse>>.Fail(404, "Event not found.");

        var days = await eventDayRepository.GetByEventIdAsync(eventId);
        var response = days.Adapt<IReadOnlyList<EventDayResponse>>();
        return ApiResponse<IReadOnlyList<EventDayResponse>>.Success(200, "Event days retrieved.", response);
    }

    public async Task<ApiResponse<EventDayResponse>> GetByIdAsync(Guid eventId, Guid dayId)
    {
        var day = await eventDayRepository.GetByIdAsync(eventId, dayId);
        if (day is null)
            return ApiResponse<EventDayResponse>.Fail(404, "Event day not found.");

        return ApiResponse<EventDayResponse>.Success(200, "Event day retrieved.", day.Adapt<EventDayResponse>());
    }

    public async Task<ApiResponse<IReadOnlyList<EventDayResponse>>> GenerateDaysAsync(
        Guid eventId, GenerateEventDaysRequest request)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity is null)
            return ApiResponse<IReadOnlyList<EventDayResponse>>.Fail(404, "Event not found.");

        if (eventEntity.StartDate is null || eventEntity.EndDate is null)
            return ApiResponse<IReadOnlyList<EventDayResponse>>.Fail(400,
                "Event must have both StartDate and EndDate set before generating days.");

        // Sync mode (force = false): add missing dates, remove orphans, preserve existing metadata.
        // DayOverrides are intentionally not applied in sync mode — they apply only to a full regeneration.
        if (!request.Force)
            return await SyncDaysAsync(
                eventId,
                eventEntity.StartDate!.Value,
                eventEntity.EndDate!.Value,
                request.TimeZoneId ?? eventEntity.TimeZoneId);

        // Regenerate mode (force = true): wipe all existing days and regenerate from scratch.
        // This is the original behavior and loses all existing metadata.

        // Resolve timezone — default to UTC if not provided
        TimeZoneInfo tz;
        try
        {
            tz = string.IsNullOrWhiteSpace(request.TimeZoneId)
                ? TimeZoneInfo.Utc
                : TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return ApiResponse<IReadOnlyList<EventDayResponse>>.Fail(400,
                $"Unknown timezone: '{request.TimeZoneId}'. Use IANA format like 'Asia/Ho_Chi_Minh'.");
        }

        // Wipe existing days before regenerating
        var existingDays = await eventDayRepository.GetByEventIdAsync(eventId);
        foreach (var existing in existingDays)
            eventDayRepository.Remove(existing);

        // Convert UTC stored dates to user's local dates before extracting DateOnly
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(eventEntity.StartDate!.Value, tz);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(eventEntity.EndDate!.Value, tz);

        var overridesMap = request.DayOverrides?
            .ToDictionary(d => d.DayNumber) ?? new Dictionary<int, DayMetadata>();

        var days = new List<EventDay>();
        var startDate = DateOnly.FromDateTime(localStart);
        var endDate = DateOnly.FromDateTime(localEnd);
        var current = startDate;
        var dayNumber = 1;

        while (current <= endDate)
        {
            overridesMap.TryGetValue(dayNumber, out var meta);
            days.Add(new EventDay
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                DayNumber = dayNumber,
                Date = current,
                Title = meta?.Title,
                Theme = meta?.Theme
            });
            current = current.AddDays(1);
            dayNumber++;
        }

        // Only set timezone if not already captured during event creation
        if (string.IsNullOrWhiteSpace(eventEntity.TimeZoneId))
        {
            eventEntity.TimeZoneId = tz.Id;
            eventRepository.UpdateEventAsync(eventEntity);
        }

        await eventDayRepository.AddRangeAsync(days);
        await eventDayRepository.SaveChangesAsync();

        var response = days.Adapt<IReadOnlyList<EventDayResponse>>();
        return ApiResponse<IReadOnlyList<EventDayResponse>>.Success(201, "Event days regenerated.", response);
    }

    public async Task<ApiResponse<IReadOnlyList<EventDayResponse>>> SyncDaysAsync(
        Guid eventId, DateTime newStartDateUtc, DateTime newEndDateUtc,
        string? timeZoneId, CancellationToken ct = default)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity is null)
            return ApiResponse<IReadOnlyList<EventDayResponse>>.Fail(404, "Event not found.");

        // Resolve timezone — prefer explicit arg, then event's stored timezone, then UTC
        var tzId = !string.IsNullOrWhiteSpace(timeZoneId) ? timeZoneId : eventEntity.TimeZoneId;
        TimeZoneInfo tz;
        try
        {
            tz = string.IsNullOrWhiteSpace(tzId)
                ? TimeZoneInfo.Utc
                : TimeZoneInfo.FindSystemTimeZoneById(tzId);
        }
        catch (TimeZoneNotFoundException)
        {
            return ApiResponse<IReadOnlyList<EventDayResponse>>.Fail(400,
                $"Unknown timezone: '{tzId}'. Use IANA format like 'Asia/Ho_Chi_Minh'.");
        }

        // Convert UTC bounds to local DateOnly values
        var newLocalStart = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(newStartDateUtc, tz));
        var newLocalEnd = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(newEndDateUtc, tz));

        // Load existing EventDays ordered by DayNumber
        var existingDays = await eventDayRepository.GetByEventIdAsync(eventId);

        // Build the new date set
        var newDateSet = new HashSet<DateOnly>();
        for (var d = newLocalStart; d <= newLocalEnd; d = d.AddDays(1))
            newDateSet.Add(d);

        var existingDateSet = existingDays.Select(d => d.Date).ToHashSet();
        var toRemoveDates = existingDateSet.Except(newDateSet).ToHashSet();
        var toAddCount = newDateSet.Except(existingDateSet).Count();

        // Short-circuit: nothing to do
        if (toRemoveDates.Count == 0 && toAddCount == 0)
        {
            var unchanged = existingDays.Adapt<IReadOnlyList<EventDayResponse>>();
            return ApiResponse<IReadOnlyList<EventDayResponse>>.Success(200, "Event days are already in sync.", unchanged);
        }

        // Delete-block check: are any sessions scheduled on dates that would be removed?
        // Sessions have no direct FK to EventDay — the relationship is derived by converting
        // Session.StartTime (UTC) to local DateOnly and matching against EventDay.Date.
        // We fetch only StartTime values (indexed by EventId) and do the conversion in memory.
        if (toRemoveDates.Count > 0)
        {
            var sessionStartTimes = await sessionRepository.GetSessionStartTimesByEventIdAsync(eventId);
            var blockedErrors = sessionStartTimes
                .Select(utc => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, tz)))
                .Where(date => toRemoveDates.Contains(date))
                .GroupBy(date => date)
                .Select(g => $"{g.Key:yyyy-MM-dd}: {g.Count()} session(s)")
                .OrderBy(s => s)
                .ToList();

            if (blockedErrors.Count > 0)
                return ApiResponse<IReadOnlyList<EventDayResponse>>.FailWithErrors(
                    409,
                    "Cannot remove event days because sessions are scheduled on those dates. " +
                    "Reassign or delete those sessions first.",
                    blockedErrors);
        }

        // Build a metadata lookup for dates that remain in the new range.
        // Title, Theme, and Description are preserved; DayNumber is reassigned sequentially (1-based).
        // We delete all existing days and re-insert the full set to avoid unique-constraint conflicts
        // that would arise from renumbering kept days in-place within a single SaveChanges call.
        var keptMeta = existingDays
            .Where(d => newDateSet.Contains(d.Date))
            .ToDictionary(
                d => d.Date,
                d => (d.Title, d.Theme, d.Description));

        // Phase 1: remove all existing days
        foreach (var day in existingDays)
            eventDayRepository.Remove(day);

        // Phase 2: insert the full new set with 1-based sequential DayNumbers in chronological order
        var newDays = new List<EventDay>();
        var dayNum = 1;
        for (var date = newLocalStart; date <= newLocalEnd; date = date.AddDays(1))
        {
            keptMeta.TryGetValue(date, out var meta);
            newDays.Add(new EventDay
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                DayNumber = dayNum++,
                Date = date,
                Title = meta.Title,
                Theme = meta.Theme,
                Description = meta.Description
            });
        }

        await eventDayRepository.AddRangeAsync(newDays);
        await eventDayRepository.SaveChangesAsync();

        var response = newDays.Adapt<IReadOnlyList<EventDayResponse>>();
        return ApiResponse<IReadOnlyList<EventDayResponse>>.Success(200, "Event days synced.", response);
    }

    public async Task<ApiResponse<EventDayResponse>> CreateAsync(Guid eventId, CreateEventDayRequest request)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity is null)
            return ApiResponse<EventDayResponse>.Fail(404, "Event not found.");

        if (eventEntity.StartDate is not null && eventEntity.EndDate is not null)
        {
            var startDate = DateOnly.FromDateTime(eventEntity.StartDate.Value);
            var endDate = DateOnly.FromDateTime(eventEntity.EndDate.Value);
            if (request.Date < startDate || request.Date > endDate)
                return ApiResponse<EventDayResponse>.Fail(400,
                    "Date must fall within the event's start and end dates.");
        }

        var duplicateExists = await eventDayRepository.ExistsAsync(eventId, request.Date);
        if (duplicateExists)
            return ApiResponse<EventDayResponse>.Fail(409,
                "An event day for this date already exists.");

        var existingDays = await eventDayRepository.GetByEventIdAsync(eventId);
        var dayNumber = existingDays.Count + 1;

        var day = new EventDay
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            DayNumber = dayNumber,
            Date = request.Date,
            Title = request.Title,
            Theme = request.Theme,
            Description = request.Description
        };

        await eventDayRepository.AddAsync(day);
        await eventDayRepository.SaveChangesAsync();

        return ApiResponse<EventDayResponse>.Success(201, "Event day created.", day.Adapt<EventDayResponse>());
    }

    public async Task<ApiResponse<EventDayResponse>> UpdateAsync(
        Guid eventId, Guid dayId, UpdateEventDayRequest request)
    {
        var day = await eventDayRepository.GetByIdAsync(eventId, dayId);
        if (day is null)
            return ApiResponse<EventDayResponse>.Fail(404, "Event day not found.");

        if (request.Title is not null)
            day.Title = request.Title;
        if (request.Theme is not null)
            day.Theme = request.Theme;
        if (request.Description is not null)
            day.Description = request.Description;

        eventDayRepository.Update(day);
        await eventDayRepository.SaveChangesAsync();

        return ApiResponse<EventDayResponse>.Success(200, "Event day updated.", day.Adapt<EventDayResponse>());
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid eventId, Guid dayId)
    {
        var day = await eventDayRepository.GetByIdAsync(eventId, dayId);
        if (day is null)
            return ApiResponse<bool>.Fail(404, "Event day not found.");

        eventDayRepository.Remove(day);
        await eventDayRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Event day deleted.", true);
    }
}

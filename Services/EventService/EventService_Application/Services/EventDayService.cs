using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class EventDayService(IEventDayRepository eventDayRepository, IEventRepository eventRepository)
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

        var alreadyExist = await eventDayRepository.AnyExistAsync(eventId);
        if (alreadyExist)
            return ApiResponse<IReadOnlyList<EventDayResponse>>.Fail(409,
                "Event days already generated. Delete existing days first.");

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

        // Convert UTC stored dates to user's local dates before extracting DateOnly
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(eventEntity.StartDate!.Value, tz);
        var localEnd   = TimeZoneInfo.ConvertTimeFromUtc(eventEntity.EndDate!.Value, tz);

        var overridesMap = request.DayOverrides?
            .ToDictionary(d => d.DayNumber) ?? new Dictionary<int, DayMetadata>();

        var days = new List<EventDay>();
        var startDate = DateOnly.FromDateTime(localStart);
        var endDate   = DateOnly.FromDateTime(localEnd);
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
        return ApiResponse<IReadOnlyList<EventDayResponse>>.Success(201, "Event days generated.", response);
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

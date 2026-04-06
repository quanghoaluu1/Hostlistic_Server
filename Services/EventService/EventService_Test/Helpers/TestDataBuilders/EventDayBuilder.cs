namespace EventService_Test.Helpers.TestDataBuilders;

public static class EventDayBuilder
{
    public static EventDay CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        int dayNumber = 1,
        DateOnly? date = null,
        string? title = "Opening Day",
        string? theme = null)
    {
        return new EventDay
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            DayNumber = dayNumber,
            Date = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Title = title,
            Theme = theme,
            Description = null
        };
    }

    public static CreateEventDayRequest CreateRequest(
        DateOnly? date = null,
        string? title = "Day 1") => new CreateEventDayRequest
    {
        Date = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        Title = title,
        Theme = null,
        Description = null
    };

    public static UpdateEventDayRequest UpdateRequest(
        string? title = "Updated Day",
        string? theme = "Innovation") => new UpdateEventDayRequest
    {
        Title = title,
        Theme = theme,
        Description = "Updated description"
    };

    public static GenerateEventDaysRequest GenerateRequest(
        string? timeZoneId = null) => new GenerateEventDaysRequest
    {
        TimeZoneId = timeZoneId,
        DayOverrides = null
    };
}

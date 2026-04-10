namespace EventService_Application.DTOs;

public record EventDayResponse
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public int DayNumber { get; init; }
    public DateOnly Date { get; init; }
    public string? Title { get; init; }
    public string? Theme { get; init; }
    public string? Description { get; init; }
}

public record CreateEventDayRequest
{
    public DateOnly Date { get; init; }
    public string? Title { get; init; }
    public string? Theme { get; init; }
    public string? Description { get; init; }
}

public record UpdateEventDayRequest
{
    public string? Title { get; init; }
    public string? Theme { get; init; }
    public string? Description { get; init; }
}

public record GenerateEventDaysRequest
{
    // IANA timezone ID, e.g. "Asia/Ho_Chi_Minh", "America/New_York"
    // Used to correctly convert UTC event dates to local calendar dates.
    // If null, falls back to UTC (existing behavior).
    public string? TimeZoneId { get; init; }

    public List<DayMetadata>? DayOverrides { get; init; }
}

public record DayMetadata
{
    public int DayNumber { get; init; }
    public string? Title { get; init; }
    public string? Theme { get; init; }
}

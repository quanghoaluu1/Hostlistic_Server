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

    // When true: wipe all existing days and regenerate from scratch (original behavior, loses metadata).
    // When false (default): delegate to SyncDaysAsync — add missing dates, remove orphans,
    // preserve metadata on overlapping dates. Returns 409 if sessions block a removal.
    // DayOverrides are only applied when Force = true.
    public bool Force { get; init; } = false;
}

public record DayMetadata
{
    public int DayNumber { get; init; }
    public string? Title { get; init; }
    public string? Theme { get; init; }
}

// Returned in the Errors list of a 409 response from SyncDaysAsync when sessions
// block the removal of specific days.
public record SyncBlockedDay(DateOnly Date, int SessionCount);

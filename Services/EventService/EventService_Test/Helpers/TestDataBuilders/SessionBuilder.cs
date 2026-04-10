namespace EventService_Test.Helpers.TestDataBuilders;

public static class SessionBuilder
{
    public static Session CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        Guid? trackId = null,
        string title = "Test Session",
        SessionStatus status = SessionStatus.Scheduled,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? totalCapacity = null,
        Track? parentTrack = null,
        Event? parentEvent = null)
    {
        var eid = eventId ?? Guid.NewGuid();
        var tid = trackId ?? Guid.NewGuid();
        var ev = parentEvent ?? EventBuilder.CreateEvent(id: eid);
        return new Session
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eid,
            TrackId = tid,
            Title = title,
            Description = "Test session description",
            Status = status,
            StartTime = startTime ?? DateTime.UtcNow.AddDays(7).AddHours(9),
            EndTime = endTime ?? DateTime.UtcNow.AddDays(7).AddHours(10),
            TotalCapacity = totalCapacity,
            SortOrder = 1,
            Event = ev,
            Track = parentTrack ?? TrackBuilder.CreateEntity(id: tid, eventId: eid, parentEvent: ev),
            Lineups = new List<Lineup>()
        };
    }

    public static CreateSessionRequest CreateRequest(
        Guid? trackId = null,
        string title = "New Session",
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? totalCapacity = null) => new CreateSessionRequest
    {
        TrackId = trackId ?? Guid.NewGuid(),
        Title = title,
        Description = "A test session",
        StartTime = startTime ?? DateTime.UtcNow.AddDays(7).AddHours(9),
        EndTime = endTime ?? DateTime.UtcNow.AddDays(7).AddHours(10),
        TotalCapacity = totalCapacity
    };

    public static UpdateSessionRequest UpdateRequest(
        string title = "Updated Session",
        DateTime? startTime = null,
        DateTime? endTime = null) => new UpdateSessionRequest
    {
        Title = title,
        Description = "Updated description",
        StartTime = startTime ?? DateTime.UtcNow.AddDays(7).AddHours(9),
        EndTime = endTime ?? DateTime.UtcNow.AddDays(7).AddHours(10)
    };
}

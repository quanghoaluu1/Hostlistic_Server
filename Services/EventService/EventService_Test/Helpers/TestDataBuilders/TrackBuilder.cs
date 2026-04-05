namespace EventService_Test.Helpers.TestDataBuilders;

public static class TrackBuilder
{
    public static Track CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        string name = "Test Track",
        string colorHex = "#6366F1",
        int sortOrder = 0,
        Event? parentEvent = null)
    {
        var eid = eventId ?? Guid.NewGuid();
        return new Track
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eid,
            Name = name,
            Description = "Test track description",
            ColorHex = colorHex,
            SortOrder = sortOrder,
            Event = parentEvent ?? EventBuilder.CreateEvent(id: eid),
            Sessions = new List<Session>()
        };
    }

    public static CreateTrackRequest CreateRequest(
        string name = "New Track",
        string colorHex = "#6366F1",
        DateTime? startTime = null,
        DateTime? endTime = null) => new CreateTrackRequest
    {
        Name = name,
        ColorHex = colorHex,
        Description = "A test track",
        StartTime = startTime,
        EndTime = endTime
    };

    public static UpdateTrackRequest UpdateRequest(
        string name = "Updated Track",
        string colorHex = "#FF5733",
        int? sortOrder = null) => new UpdateTrackRequest
    {
        Name = name,
        ColorHex = colorHex,
        SortOrder = sortOrder
    };
}

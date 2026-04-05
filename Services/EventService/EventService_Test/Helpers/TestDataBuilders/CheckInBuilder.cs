namespace EventService_Test.Helpers.TestDataBuilders;

public static class CheckInBuilder
{
    public static CheckIn CreateEntity(
        Guid? id = null,
        Guid? ticketId = null,
        Guid? eventId = null,
        Guid? sessionId = null,
        Guid? checkedBy = null,
        string location = "Main Gate",
        CheckInType type = CheckInType.EventLevel)
    {
        return new CheckIn
        {
            Id = id ?? Guid.NewGuid(),
            TicketId = ticketId ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            SessionId = sessionId,
            CheckedBy = checkedBy ?? Guid.NewGuid(),
            CheckedInAt = DateTime.UtcNow,
            CheckInLocation = location,
            CheckInType = type
        };
    }

    public static CreateCheckInRequest CreateRequest(
        Guid? ticketId = null,
        Guid? eventId = null,
        string location = "Entrance") => new CreateCheckInRequest
    {
        TicketId = ticketId ?? Guid.NewGuid(),
        EventId = eventId ?? Guid.NewGuid(),
        SessionId = null,
        CheckInLocation = location,
        CheckInType = CheckInType.EventLevel
    };

    public static UpdateCheckInRequest UpdateRequest(
        string location = "Side Gate",
        CheckInType type = CheckInType.EventLevel) => new UpdateCheckInRequest
    {
        CheckInLocation = location,
        CheckInType = type
    };
}

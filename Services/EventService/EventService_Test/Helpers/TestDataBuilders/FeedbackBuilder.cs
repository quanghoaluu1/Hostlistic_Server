namespace EventService_Test.Helpers.TestDataBuilders;

public static class FeedbackBuilder
{
    public static Feedback CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        Guid? sessionId = null,
        Guid? userId = null,
        int rating = 4,
        string comment = "Great session!")
    {
        return new Feedback
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            SessionId = sessionId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static FeedbackDto CreateDto(
        Guid? eventId = null,
        Guid? sessionId = null,
        Guid? userId = null,
        int rating = 4,
        string comment = "Very informative") => new FeedbackDto
    {
        Id = Guid.NewGuid(),
        EventId = eventId ?? Guid.NewGuid(),
        SessionId = sessionId ?? Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        Rating = rating,
        Comment = comment
    };

    public static UpdateFeedbackDto UpdateRequest(
        int rating = 5,
        string comment = "Outstanding!") => new UpdateFeedbackDto
    {
        Rating = rating,
        Comment = comment
    };
}

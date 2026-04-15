namespace EventService_Test.Helpers.TestDataBuilders;

public static class FeedbackBuilder
{
    public static Feedback CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        Guid? userId = null,
        int rating = 4,
        string comment = "Great event!",
        string userFullName = "Test User")
    {
        return new Feedback
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            UserFullName = userFullName,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static CreateFeedbackDto CreateDto(
        Guid? eventId = null,
        int rating = 4,
        string comment = "Very informative") => new CreateFeedbackDto
    {
        EventId = eventId ?? Guid.NewGuid(),
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

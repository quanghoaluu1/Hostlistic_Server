using AIService_Application.DTOs.Responses;

namespace AIService_Test.Helpers.TestDataBuilders;

public class EventBuilder
{
    public static EventDetailDto CreateEventDetail(
        Guid? id = null,
        string title = "Test Event")
    {
        return new EventDetailDto
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(8),
            Tracks = Array.Empty<TrackDetailDto>()
        };
    }
}

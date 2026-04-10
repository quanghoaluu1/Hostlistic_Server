using EventService_Domain;

namespace EventService_Test.Helpers.TestDataBuilders;

public class EventTypeBuilder
{
    public static EventType CreateEventType(
        Guid? id = null,
        string name = "Conference",
        bool isActive = true)
    {
        return new EventType
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            IsActive = isActive
        };
    }
}
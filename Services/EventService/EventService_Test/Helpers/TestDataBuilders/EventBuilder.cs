namespace EventService_Test.Helpers.TestDataBuilders;

public class EventBuilder
{
    public static Event CreateEvent(
        Guid? id = null,
        Guid? organizerId = null,
        string title = "Test Event",
        EventStatus status = EventStatus.Draft,
        int? totalCapacity = 100)
    {
        var eventId = id ?? Guid.NewGuid();
        return new Event
        {
            Id = eventId,
            OrganizerId = organizerId ?? Guid.NewGuid(),
            Title = title,
            EventStatus = status,
            TotalCapacity = totalCapacity,
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(8),
            IsPublic = false,
            Location = "HCMC",
            EventMode = EventMode.Offline,
            Tracks = new List<Track>(),
            EventTeamMembers = new List<EventTeamMember>(),
            Sessions = new List<Session>()
        };
    }
    public static EventRequestDto CreateEventRequest(
        string title = "New Event",
        int? totalCapacity = 100,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return new EventRequestDto(
            Title: title,
            TotalCapacity: totalCapacity,
            StartDate: startDate ?? DateTime.UtcNow.AddDays(7),
            EndDate: endDate ?? DateTime.UtcNow.AddDays(8),
            EventMode: EventMode.Offline,
            Location: "HCMC"
        );
    }}
namespace EventService_Test.Helpers.TestDataBuilders;

public static class VenueBuilder
{
    public static Venue CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        string name = "Test Hall",
        int capacity = 200,
        string? layoutPublicId = null)
    {
        var eid = eventId ?? Guid.NewGuid();
        return new Venue
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eid,
            Name = name,
            Description = "A test venue",
            Capacity = capacity,
            LayoutUrl = layoutPublicId is not null ? "https://res.cloudinary.com/test/image" : null,
            LayoutPublicId = layoutPublicId,
            Sessions = new List<Session>()
        };
    }

    public static CreateVenueRequest CreateRequest(
        string name = "New Hall",
        int capacity = 300) => new CreateVenueRequest(
        Name: name,
        Description: "A test venue hall",
        Capacity: capacity,
        LayoutImage: null);

    public static UpdateVenueRequest UpdateRequest(
        string? name = "Updated Hall",
        int? capacity = null) => new UpdateVenueRequest(
        Name: name,
        Description: null,
        Capacity: capacity,
        LayoutImage: null,
        RemoveLayout: false);
}

namespace EventService_Test.Helpers.TestDataBuilders;

public static class TalentBuilder
{
    public static Talent CreateEntity(
        Guid? id = null,
        string name = "John Doe",
        string type = "Speaker",
        string? email = "john@example.com",
        bool hasLineups = false)
    {
        return new Talent
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Bio = "A seasoned professional",
            AvatarUrl = "https://example.com/avatar.jpg",
            Type = type,
            Organization = "Tech Corp",
            Email = email,
            Lineups = hasLineups
                ? new List<Lineup> { new Lineup { Id = Guid.NewGuid() } }
                : new List<Lineup>()
        };
    }

    public static CreateTalentDto CreateRequest(
        string name = "Jane Smith",
        string type = "Panelist",
        string email = "jane@example.com") => new CreateTalentDto
    {
        Name = name,
        Bio = "Expert speaker",
        AvatarUrl = "https://example.com/avatar.jpg",
        Type = type,
        Organization = "Innovation Ltd",
        Email = email
    };

    public static UpdateTalentDto UpdateRequest(
        string? name = "Updated Name",
        string? bio = "Updated bio") => new UpdateTalentDto
    {
        Name = name,
        Bio = bio,
        Organization = "Updated Org"
    };
}

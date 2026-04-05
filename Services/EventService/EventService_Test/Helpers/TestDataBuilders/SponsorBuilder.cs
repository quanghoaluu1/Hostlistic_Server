namespace EventService_Test.Helpers.TestDataBuilders;

public static class SponsorBuilder
{
    public static Sponsor CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        Guid? tierId = null,
        string name = "ACME Corp",
        string? logoUrl = "https://example.com/logo.png")
    {
        var tid = tierId ?? Guid.NewGuid();
        return new Sponsor
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            Name = name,
            LogoUrl = logoUrl ?? string.Empty,
            Description = "A great sponsor",
            WebsiteUrl = "https://acme.example.com",
            TierId = tid,
            Tier = new SponsorTier { Id = tid, Name = "Gold", Priority = 1 },
            SponsorInteractions = new List<SponsorInteraction>()
        };
    }

    public static CreateSponsorDto CreateRequest(
        Guid? eventId = null,
        Guid? tierId = null,
        string name = "New Sponsor") => new CreateSponsorDto
    {
        EventId = eventId ?? Guid.NewGuid(),
        TierId = tierId ?? Guid.NewGuid(),
        Name = name,
        LogoUrl = "https://example.com/logo.png",
        Description = "Sponsor description",
        WebsiteUrl = "https://sponsor.example.com"
    };

    public static UpdateSponsorDto UpdateRequest(
        string? name = "Updated Sponsor",
        Guid? tierId = null) => new UpdateSponsorDto
    {
        Name = name,
        LogoUrl = null,
        Description = null,
        WebsiteUrl = null,
        TierId = tierId
    };
}

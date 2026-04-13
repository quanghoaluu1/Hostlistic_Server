namespace NotificationService_Application.Dtos;

public sealed record HolderTicketEmailModel
{
    public required string HolderName { get; init; }
    public required string HolderEmail { get; init; }
    public required string BuyerName { get; init; }
    public required string EventName { get; init; }
    public required DateTime EventDate { get; init; }
    public required string EventLocation { get; init; }
    public required string PortalUrl { get; init; }
    public string? LogoUrl { get; init; }
    public required IReadOnlyList<TicketEmailInfo> Tickets { get; init; }
}

namespace NotificationService_Application.Dtos.ServiceClientDtos;

public sealed record EventDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Location { get; init; }
    public string? TimeZoneId { get; init; }
    public string? Status { get; init; }
}
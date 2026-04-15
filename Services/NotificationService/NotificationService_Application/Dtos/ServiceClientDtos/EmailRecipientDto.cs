namespace NotificationService_Application.Dtos.ServiceClientDtos;

public sealed record EmailRecipientDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string TicketTypeName { get; init; } = string.Empty;
}
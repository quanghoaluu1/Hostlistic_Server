namespace BookingService_Application.DTOs;

public sealed record EmailRecipientDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string TicketTypeName { get; init; } = string.Empty;
}

public sealed record GetEmailRecipientsRequest
{
    public int RecipientGroup { get; init; } = 0;
    public List<Guid>? TicketTypeIds { get; init; }
    public List<Guid>? SpecificUserIds { get; init; }
    public DateTime? PurchasedAfter { get; init; }
}
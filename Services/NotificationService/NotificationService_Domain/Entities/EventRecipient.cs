namespace NotificationService_Domain.Entities;

public class EventRecipient
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? TicketTypeId { get; set; }
    public string? TicketTypeName { get; set; }
    public DateTime BookingConfirmedAt { get; set; }
    public bool IsCheckedIn { get; set; } = false;
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}
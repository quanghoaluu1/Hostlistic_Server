using NotificationService_Domain.Enums;

namespace NotificationService_Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid? EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public RecipientType RecipientType { get; set; }
    public NotificationTargetData? TargetData { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public NotificationStatus Status { get; set; } = NotificationStatus.Scheduled;
    public string? ErrorMessage { get; set; } = string.Empty;
    
    public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}

public class NotificationTargetData
{
    public List<Guid>? UserIds { get; set; }
    public string? TicketType { get; set; }
    public DateTime? PurchasedBefore { get; set; }
}
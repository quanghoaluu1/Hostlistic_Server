using NotificationService_Domain.Enums;

namespace NotificationService_Application.DTOs;

public class CreateNotificationRequest
{
    public Guid? EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public RecipientType RecipientType { get; set; }
    public NotificationTargetDataDto? TargetData { get; set; }
    public DateTime ScheduledDate { get; set; }
}

public class UpdateNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public RecipientType RecipientType { get; set; }
    public NotificationTargetDataDto? TargetData { get; set; }
    public DateTime ScheduledDate { get; set; }
    public NotificationStatus Status { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid? EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public RecipientType RecipientType { get; set; }
    public NotificationTargetDataDto? TargetData { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime SentAt { get; set; }
    public NotificationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NotificationTargetDataDto
{
    public List<Guid>? UserIds { get; set; }
    public string? TicketType { get; set; }
    public DateTime? PurchasedBefore { get; set; }
}

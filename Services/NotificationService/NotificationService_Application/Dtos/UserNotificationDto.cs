using NotificationService_Domain.Enums;

namespace NotificationService_Application.DTOs;

public class CreateUserNotificationRequest
{
    public Guid UserId { get; set; }
    public Guid NotificationId { get; set; }
}

public class UpdateUserNotificationRequest
{
    public bool IsRead { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public string? DeliveryError { get; set; }
}

public class UserNotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid NotificationId { get; set; }
    public bool IsRead { get; set; }
    public DateTime ReadAt { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public string? DeliveryError { get; set; }
}

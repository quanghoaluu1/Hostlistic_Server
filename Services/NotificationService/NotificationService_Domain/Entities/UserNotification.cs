using NotificationService_Domain.Enums;

namespace NotificationService_Domain.Entities;

public class UserNotification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid NotificationId { get; set; }
    public bool IsRead { get; set; }
    public DateTime ReadAt { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public string? DeliveryError { get; set; }
}
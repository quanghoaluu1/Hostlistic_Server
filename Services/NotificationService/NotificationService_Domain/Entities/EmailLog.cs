using NotificationService_Domain.Enums;

namespace NotificationService_Domain.Entities;

public class EmailLog
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid SentTo { get; set; }
    public DateTime SentAt { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public string? ErrorMessage { get; set; } = string.Empty;
    public bool IsOpened { get; set; } = false;
    public bool IsClicked { get; set; } = false;
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
}
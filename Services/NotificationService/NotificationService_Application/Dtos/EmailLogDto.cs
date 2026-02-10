using NotificationService_Domain.Enums;

namespace NotificationService_Application.DTOs;

public class CreateEmailLogRequest
{
    public Guid CampaignId { get; set; }
    public Guid SentTo { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
}

public class UpdateEmailLogRequest
{
    public DeliveryStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsOpened { get; set; }
    public bool IsClicked { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
}

public class EmailLogDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid SentTo { get; set; }
    public DateTime SentAt { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public DeliveryStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsOpened { get; set; }
    public bool IsClicked { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
}

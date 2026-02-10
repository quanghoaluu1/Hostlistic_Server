using NotificationService_Domain.Enums;

namespace NotificationService_Application.DTOs;

public class CreateEmailCampaignRequest
{
    public Guid? EventId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public RecipientGroup RecipientGroup { get; set; }
    public EmailTargetFilterDto? TargetFilter { get; set; }
}

public class UpdateEmailCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public EmailCampaignStatus Status { get; set; }
    public RecipientGroup RecipientGroup { get; set; }
    public EmailTargetFilterDto? TargetFilter { get; set; }
}

public class EmailCampaignDto
{
    public Guid Id { get; set; }
    public Guid? EventId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public EmailCampaignStatus Status { get; set; }
    public RecipientGroup RecipientGroup { get; set; }
    public EmailTargetFilterDto? TargetFilter { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class EmailTargetFilterDto
{
    public List<Guid>? TicketTypeIds { get; set; }
    public DateTime? PurchasedAfter { get; set; }
    public List<Guid>? SpecificUserIds { get; set; }
}

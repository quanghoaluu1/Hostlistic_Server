using Common;
using NotificationService_Domain.Enums;

namespace NotificationService_Domain.Entities;

public class EmailCampaign : BaseClass
{
    public Guid? EventId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public EmailCampaignStatus Status { get; set; } = EmailCampaignStatus.Draft;
    public RecipientGroup RecipientGroup { get; set; }
    public EmailTargetFilter? TargetFilter { get; set; }
    
    public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
}
public class EmailTargetFilter
{
    // Dành cho lọc theo loại vé
    public List<Guid>? TicketTypeIds { get; set; } 
        
    // Dành cho lọc theo thời gian mua (vd: mua trước ngày X)
    public DateTime? PurchasedAfter { get; set; }

    // Dành cho danh sách thủ công (ManualList)
    public List<Guid>? SpecificUserIds { get; set; }
}
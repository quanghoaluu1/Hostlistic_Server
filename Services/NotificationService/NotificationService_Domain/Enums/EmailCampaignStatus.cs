namespace NotificationService_Domain.Enums;

public enum EmailCampaignStatus
{
    Draft,
    Sending,
    Paused,       
    Completed,
    Failed,
    Cancelled
}
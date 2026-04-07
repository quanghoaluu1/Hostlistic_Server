namespace NotificationService_Application.Dtos;

    public class CampaignSendResponse
    {
        public Guid CampaignId { get; set; }
        public int RecipientCount { get; set; }
        public string Status { get; set; } = "Sending";
        public string Message { get; set; } = string.Empty;
    }
 
    /// <summary>Response DTO for campaign status polling.</summary>
    public class CampaignStatusResponse
    {
        public Guid CampaignId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
 
    /// <summary>Preview DTO — shows how many recipients before committing to send.</summary>
    public class CampaignPreviewResponse
    {
        public Guid CampaignId { get; set; }
        public int RecipientCount { get; set; }
        public int DailyQuotaRemaining { get; set; }
        public bool CanSend { get; set; }
        public string? Warning { get; set; }
    }

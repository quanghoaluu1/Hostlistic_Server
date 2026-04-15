namespace NotificationService_Application.Dtos;

public sealed record ReminderEmailContent
{
    /// <summary>
    /// Must match one of: reminder_7day, reminder_3day, reminder_1day, reminder_sameday
    /// </summary>
    public string EmailType { get; init; } = string.Empty;
    
    public string Subject { get; init; } = string.Empty;
    
    public string HtmlBody { get; init; } = string.Empty;
}

public sealed record SetupAutoRemindersRequest
{
    public List<ReminderEmailContent> Reminders { get; init; } = [];
    public bool OverwriteExisting { get; init; } = false;
}

public sealed record SetupRemindersResult
{
    public List<ReminderCampaignInfo> CreatedReminders { get; init; } = [];
    public int SkippedCount { get; init; }
}

public sealed record ReminderCampaignInfo
{
    public Guid CampaignId { get; init; }
    public string EmailType { get; init; } = string.Empty;
    public DateTime ScheduledAtUtc { get; init; }
    public string HangfireJobId { get; init; } = string.Empty;
}
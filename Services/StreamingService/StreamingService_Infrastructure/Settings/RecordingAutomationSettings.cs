namespace StreamingService_Infrastructure.Settings;

public sealed class RecordingAutomationSettings
{
    public const string SectionName = "RecordingAutomation";

    public bool Enabled { get; set; } = true;
    public string InboxPath { get; set; } = "incoming";
    public string ProcessedPath { get; set; } = "processed";
    public string FailedPath { get; set; } = "failed";
    public int PollIntervalSeconds { get; set; } = 15;
    public int FileStableSeconds { get; set; } = 15;
}

namespace StreamingService_Infrastructure.Settings;

public class LiveKitSettings
{
    public const string SectionName = "LiveKit";
    
    public string ServerUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

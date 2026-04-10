namespace StreamingService_Infrastructure.Settings;

public sealed class RecordingStorageSettings
{
    public const string SectionName = "RecordingStorage";

    public string RootPath { get; set; } = "recordings";
    public string RequestPath { get; set; } = "/recordings";
    public string? PublicBaseUrl { get; set; }
}

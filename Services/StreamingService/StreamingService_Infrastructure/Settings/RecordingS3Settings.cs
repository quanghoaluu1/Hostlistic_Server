namespace StreamingService_Infrastructure.Settings;

public sealed class RecordingS3Settings
{
    public const string SectionName = "RecordingS3";

    public bool Enabled { get; set; }

    /// <summary>
    /// S3 endpoint URL, e.g. "http://minio:9000" or "https://minio.example.com".
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Bucket to store recordings in.
    /// </summary>
    public string Bucket { get; set; } = "recordings";

    /// <summary>
    /// Use SSL for the S3 endpoint.
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Force path-style addressing (recommended for MinIO).
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;

    /// <summary>
    /// Optional: a public base URL used to construct playback links.
    /// Example: "https://cdn.example.com/recordings" or "https://minio.example.com/recordings".
    /// If empty, playback URL will be built from Endpoint + Bucket + object key.
    /// </summary>
    public string? PublicBaseUrl { get; set; }
}


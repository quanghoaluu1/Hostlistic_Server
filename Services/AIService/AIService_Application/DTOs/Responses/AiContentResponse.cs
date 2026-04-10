namespace AIService_Application.DTOs.Responses;

public class AiContentResponse
{
    public Guid RequestId { get; set; }
    public Guid ContentId { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public string PlainContent { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; } = true;
    public AiMetadataDto Metadata { get; set; } = new();
}
public class AiMetadataDto
{
    public string Model { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public long LatencyMs { get; set; }
    /// <summary>
    /// "rich" | "partial" | "minimal" — source data completeness level.
    /// Null for non-speaker-intro endpoints.
    /// </summary>
    public string? DataQuality { get; init; }

    /// <summary>
    /// True when the AI output is based on minimal data and should be
    /// reviewed/edited by the organizer before publishing.
    /// </summary>
    public bool NeedsReview { get; init; }
}
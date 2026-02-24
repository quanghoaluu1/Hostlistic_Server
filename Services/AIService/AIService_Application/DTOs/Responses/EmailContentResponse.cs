namespace AIService_Application.DTOs.Responses;

public class EmailContentResponse
{
    public Guid ContentId { get; init; }
    public Guid RequestId { get; init; }

    /// <summary>
    /// Parsed subject line extracted from AI response
    /// </summary>
    public string SubjectLine { get; init; } = string.Empty;

    /// <summary>
    /// HTML body for rich email editors / TipTap
    /// </summary>
    public string HtmlBody { get; init; } = string.Empty;

    /// <summary>
    /// Plain text version for fallback / preview
    /// </summary>
    public string PlainTextBody { get; init; } = string.Empty;

    public string EmailType { get; init; } = string.Empty;
    public string Tone { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int TokensUsed { get; init; }
    public long GenerationTimeMs { get; init; }
}
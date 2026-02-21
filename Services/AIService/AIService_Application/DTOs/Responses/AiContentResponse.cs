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
}
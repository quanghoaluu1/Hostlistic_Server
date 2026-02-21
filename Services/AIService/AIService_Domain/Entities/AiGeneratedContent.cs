using System.ComponentModel.DataAnnotations.Schema;

namespace AIService_Domain.Entities;

public class AiGeneratedContent
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public string PlainContent { get; set; } = string.Empty;  
    public string Model { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public long LatencyMs { get; set; }
    public bool IsChosen { get; set; } = false;
    public string? FinalEditedHtml { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ChosenAt { get; set; }
    
    public AiRequest Request { get; set; } = null!;
}
using AIService_Domain.Enum;

namespace AIService_Application.DTOs.Responses;

public class PromptTemplateResponse
{
    public Guid Id { get; set; }
    public PromptTemplateKey TemplateKey { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PromptCategory Category { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPromptTemplate { get; set; } = string.Empty;
    public double DefaultTemperature { get; set; }
    public int DefaultMaxTokens { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

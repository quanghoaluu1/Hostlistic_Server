using AIService_Domain.Enum;

namespace AIService_Domain.Entities;

public class PromptTemplate
{
    public Guid Id { get; set; }

    public PromptTemplateKey TemplateKey { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PromptCategory Category { get; set; }

    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPromptTemplate { get; set; } = string.Empty;

    public double DefaultTemperature { get; set; } = 0.7f;
    public int DefaultMaxTokens { get; set; } = 1000;

    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
using System.ComponentModel.DataAnnotations;
using AIService_Domain.Enum;

namespace AIService_Application.DTOs.Requests;

public class CreatePromptTemplateRequest
{
    [Required]
    public PromptTemplateKey TemplateKey { get; set; }

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public PromptCategory Category { get; set; }

    [Required]
    public string SystemPrompt { get; set; } = string.Empty;

    [Required]
    public string UserPromptTemplate { get; set; } = string.Empty;

    public double DefaultTemperature { get; set; } = 0.7f;

    [Range(1, 32000)]
    public int DefaultMaxTokens { get; set; } = 1000;
}

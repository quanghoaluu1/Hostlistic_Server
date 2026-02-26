using System.ComponentModel.DataAnnotations;
using AIService_Domain.Enum;

namespace AIService_Application.DTOs.Requests;

public class UpdatePromptTemplateRequest
{
    [MaxLength(200)]
    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public PromptCategory? Category { get; set; }

    public string? SystemPrompt { get; set; }

    public string? UserPromptTemplate { get; set; }

    public double? DefaultTemperature { get; set; }

    [Range(1, 32000)]
    public int? DefaultMaxTokens { get; set; }

    public bool? IsActive { get; set; }
}

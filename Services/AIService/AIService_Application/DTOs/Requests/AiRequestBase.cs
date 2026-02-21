using System.ComponentModel.DataAnnotations;

namespace AIService_Application.DTOs.Requests;

public abstract class AiRequestBase
{
    [Required]
    public Guid EventId { get; set; }
    [RegularExpression("^(formal|friendly|marketing)$")]
    public string Tone { get; set; } = "formal";
    [RegularExpression("^(en|vi)$")]
    public string Language { get; set; } = "en";
    public string? AdditionalContext { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace AIService_Application.DTOs.Requests;

public record GenerateSocialPostRequest
{
    [Required]
    public Guid EventId { get; set; }
    
    [Required]
    [RegularExpression("^(facebook|twitter|linkedin|instagram)$",
        ErrorMessage = "Platform must be one of: facebook, twitter, linkedin, instagram")]
    public string Platform { get; init; } = "facebook";
    
    [Required]
    [RegularExpression("^(short|medium|long)$",
        ErrorMessage = "Length must be one of: short, medium, long")]
    public string Length { get; init; } = "medium";
    
    [Required]
    [RegularExpression("^(formal|friendly|marketing)$",
        ErrorMessage = "Tone must be one of: formal, friendly, marketing")]
    public string Tone { get; init; } = "marketing";
    
    [Required]
    [RegularExpression("^(en|vi)$", ErrorMessage = "Language must be 'en' or 'vi'")]
    public string Language { get; init; } = "en";
    
    [MaxLength(500)]
    public string? Hashtags { get; init; }
    
    /// Optional: explicit CTA (e.g., "Register now at hostlistic.tech")
    [MaxLength(500)]
    public string? CallToAction { get; init; }
    
    /// Optional: key selling points or highlights to emphasize
    [MaxLength(1000)]
    public string? KeyHighlights { get; init; }
};
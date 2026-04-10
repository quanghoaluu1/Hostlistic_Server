using System.ComponentModel.DataAnnotations;

namespace AIService_Application.DTOs.Requests;

public class GenerateSessionAbstractRequest
{
    [Required]
    public Guid EventId { get; init; }

    [Required]
    public Guid SessionId { get; init; }

    [Required]
    [RegularExpression("^(formal|friendly|marketing)$",
        ErrorMessage = "Tone must be one of: formal, friendly, marketing")]
    public string Tone { get; init; } = "formal";

    [Required]
    [RegularExpression("^(en|vi)$", ErrorMessage = "Language must be 'en' or 'vi'")]
    public string Language { get; init; } = "en";
    
    /// <summary>
    /// "from_metadata" = generate abstract from session title, speakers, track info.
    /// "expand" = organizer provides a short description, AI expands into full abstract.
    /// </summary>
    [Required]
    [RegularExpression("^(from_metadata|expand)$",
        ErrorMessage = "Mode must be 'from_metadata' or 'expand'")]
    public string Mode { get; init; } = "from_metadata";

    /// <summary>
    /// Required when mode = "expand". Organizer's short description or bullet points
    /// that the AI will expand into a professional session abstract.
    /// </summary>
    [MinLength(20)]
    [MaxLength(3000)]
    public string? SourceText { get; init; }

    /// <summary>
    /// Target audience context: "general", "technical", "executive"
    /// </summary>
    [RegularExpression("^(general|technical|executive)$",
        ErrorMessage = "TargetAudience must be one of: general, technical, executive")]
    public string TargetAudience { get; init; } = "general";

    /// <summary>
    /// Optional: key takeaways the organizer wants to highlight
    /// </summary>
    [MaxLength(1000)]
    public string? KeyTakeaways { get; init; }

    /// <summary>
    /// Optional: additional context about the session
    /// </summary>
    [MaxLength(1000)]
    public string? AdditionalContext { get; init; }
}
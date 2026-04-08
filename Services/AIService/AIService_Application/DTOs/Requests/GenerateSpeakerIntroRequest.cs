using System.ComponentModel.DataAnnotations;

namespace AIService_Application.DTOs.Requests;

public class GenerateSpeakerIntroRequest
{
    [Required]
    public Guid EventId { get; init; }

    [Required]
    public Guid TalentId { get; init; }

    [Required]
    [RegularExpression("^(formal|friendly|marketing)$",
        ErrorMessage = "Tone must be one of: formal, friendly, marketing")]
    public string Tone { get; init; } = "formal";

    [Required]
    [RegularExpression("^(en|vi)$", ErrorMessage = "Language must be 'en' or 'vi'")]
    public string Language { get; init; } = "en";

    /// <summary>
    /// "from_name" = generate intro from talent metadata only.
    /// "summarize" = condense the provided sourceText into a short bio.
    /// </summary>
    [Required]
    [RegularExpression("^(from_name|summarize)$",
        ErrorMessage = "Mode must be 'from_name' or 'summarize'")]
    public string Mode { get; init; } = "from_name";

    /// <summary>
    /// Required when mode = "summarize". The organizer pastes a long CV,
    /// LinkedIn bio, or full speaker description for the AI to condense.
    /// </summary>
    [MaxLength(5000)]
    public string? SourceText { get; init; }
    public bool AllowWebKnowledge { get; init; } = false;

    /// <summary>
    /// Optional: additional details the organizer wants highlighted
    /// (e.g., "Emphasize their work in AI ethics")
    /// </summary>
    [MaxLength(1000)]
    public string? AdditionalContext { get; init; }
}
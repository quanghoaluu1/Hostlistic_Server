using System.ComponentModel.DataAnnotations;

namespace AIService_Application.DTOs.Requests;

public class GenerateEmailRequest
{
    [Required]
    public Guid EventId { get; init; }

    [Required]
    [AllowedValues("invitation", "reminder_7day", "reminder_3day", 
        "reminder_1day", "reminder_sameday", "confirmation", 
        "post_event_thankyou")]
    public string EmailType { get; init; } = "invitation";

    [Required]
    [AllowedValues("formal", "friendly", "marketing")]
    public string Tone { get; init; } = "formal";

    [Required]
    [AllowedValues("English", "Vietnamese")]
    public string Language { get; init; } = "English";

    // --- Invitation-specific fields ---
    [AllowedValues("general", "vip", "speakers", "sponsors", "students", "professionals")]
    public string? RecipientType { get; init; }

    public string? TargetAudience { get; init; }
    public string? SellingPoints { get; init; }
    public string? EarlyBirdDeadline { get; init; }
    public string? EarlyBirdDiscount { get; init; }
    public string? TicketPrice { get; init; }

    // --- Reminder-specific fields ---
    public string? CheckinInstructions { get; init; }
    public string? PreparationNotes { get; init; }
    public string? AgendaHighlights { get; init; }
    public string? AttendeeName { get; init; }
    public string? TicketType { get; init; }
}
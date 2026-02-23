using System.ComponentModel.DataAnnotations.Schema;
using AIService_Domain.Enum;

namespace AIService_Domain.Entities;

public class AiRequest
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid CreatedBy { get; set; }
    public AiRequesttype RequestType { get; set; }
    public string? Tone { get; set; }              // formal, friendly, marketing
    public string Language { get; set; } = "en";   // en, vi
    public string? TargetAudience { get; set; }
    public string? Objectives { get; set; }
    public string? Keywords { get; set; }          // comma-separated hoặc JSON array
    public string? AdditionalContext { get; set; } // free-text bổ sung
    public AiRequestStatus Status { get; set; } = AiRequestStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    public ICollection<AiGeneratedContent>? GeneratedContents { get; set; } = new List<AiGeneratedContent>();
}
using System.ComponentModel.DataAnnotations.Schema;
using AIService_Domain.Enum;

namespace AIService_Domain.Entities;

public class AiRequest
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid CreatedBy { get; set; }
    public AiRequesttype RequestType { get; set; }
    [Column(TypeName = "jsonb")]
    public string InputParams { get; set; } = string.Empty;
    public AiRequestStatus Status { get; set; } = AiRequestStatus.Pending;
    public string? ErrorMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<AiGeneratedContent>? GeneratedContents { get; set; } = new List<AiGeneratedContent>();
}
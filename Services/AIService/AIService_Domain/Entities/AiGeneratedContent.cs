using System.ComponentModel.DataAnnotations.Schema;

namespace AIService_Domain.Entities;

public class AiGeneratedContent
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsChosen { get; set; } = false;
    public string? FinalEditedText { get; set; } = string.Empty;
}
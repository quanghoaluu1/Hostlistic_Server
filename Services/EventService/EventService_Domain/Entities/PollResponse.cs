using System.ComponentModel.DataAnnotations.Schema;

namespace EventService_Domain.Entities;

public class PollResponse
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid? UserId { get; set; }
    public int[] SelectedOptionId { get; set; } = [];
    public string? AnswerText { get; set; } = string.Empty;
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property to parent
    [ForeignKey("PollId")]
    public virtual Poll Poll { get; set; } = null!;
}
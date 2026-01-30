using System.ComponentModel.DataAnnotations.Schema;

namespace EventService_Domain.Entities;

public class QaVote
{
    public Guid UserId { get; set; }
    public Guid QaQuestionId { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property to parent
    [ForeignKey("QaQuestionId")]
    public virtual QaQuestion QaQuestion { get; set; } = null!;
}
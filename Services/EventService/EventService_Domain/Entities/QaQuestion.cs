using System.ComponentModel.DataAnnotations.Schema;
using Common;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class QaQuestion : BaseClass
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QaStatus Status { get; set; }
    public int UpVotes { get; set; }
    public int DurationInSecond { get; set; }
    public TimeSpan AskedAt { get; set; }
    
    // Navigation property to parent
    [ForeignKey("SessionId")]
    public virtual Session Session { get; set; } = null!;
    
    // Navigation properties to children
    public ICollection<QaVote> Votes { get; set; } = new List<QaVote>();
}
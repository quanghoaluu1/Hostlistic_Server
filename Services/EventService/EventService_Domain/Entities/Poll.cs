using System.ComponentModel.DataAnnotations.Schema;
using Common;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class Poll : BaseClass
{
    public Guid SessionId { get; set; }
    public string Question { get; set; } = string.Empty;
    
    [Column(TypeName = "jsonb")]
    public List<PollOption> Options { get; set; } = [];
    
    [Column(TypeName = "jsonb")]
    public List<int> CorrectAnswers { get; set; } = [];
    public PollType Type { get; set; }
    public bool IsPrivate { get; set; }
    public int? DurationInSecond { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<PollResponse> PollResponses { get; set; } = new List<PollResponse>();
}

public class PollOption
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? ImageUrl { get; set; } = string.Empty;
}
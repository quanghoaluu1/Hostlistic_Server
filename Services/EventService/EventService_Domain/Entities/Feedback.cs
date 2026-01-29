using Common;

namespace EventService_Domain.Entities;

public class Feedback : BaseClass
{
    public Guid EventId { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
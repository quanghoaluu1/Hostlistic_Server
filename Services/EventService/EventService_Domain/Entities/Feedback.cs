using System.ComponentModel.DataAnnotations.Schema;
using Common;

namespace EventService_Domain.Entities;

public class Feedback : BaseClass
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
}
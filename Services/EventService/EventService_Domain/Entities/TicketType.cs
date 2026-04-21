using System.ComponentModel.DataAnnotations.Schema;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class TicketType
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid? SessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public string? Description { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
    public int QuantitySold { get; set; }
    public DateTime SaleStartDate { get; set; }
    public DateTime SaleEndTime { get; set; } //Thời gian kết thúc trước khi đóng vé
    public int MinPerOrder { get; set; }
    public int MaxPerOrder { get; set; }
    public bool IsRequireHolderInfo { get; set; }
    public TicketTypeStatus Status { get; set; } = TicketTypeStatus.Active;
    public SaleChannel SaleChannel { get; set; }

    // Streaming benefits
    /// <summary>Maximum number of Q&amp;A questions a holder of this ticket type may ask. Null means unlimited.</summary>
    public int? MaxQaQuestions { get; private set; }

    /// <summary>Specific Track IDs this ticket type grants access to. Empty list means no track restriction.</summary>
    public List<Guid> AllowedTrackIds { get; private set; } = new();

    // Navigation properties to parent
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;

    [ForeignKey("SessionId")]
    public virtual Session? Session { get; set; }

    /// <summary>
    /// Safely updates the streaming benefit properties of this ticket type.
    /// </summary>
    /// <param name="maxQaQuestions">Maximum Q&amp;A questions allowed; pass null for unlimited.</param>
    /// <param name="allowedTrackIds">Allowed track IDs; pass null or empty to remove restrictions.</param>
    public void UpdateStreamingBenefits(int? maxQaQuestions, IEnumerable<Guid>? allowedTrackIds)
    {
        MaxQaQuestions = maxQaQuestions;
        AllowedTrackIds = allowedTrackIds is not null
            ? new List<Guid>(allowedTrackIds)
            : new List<Guid>();
    }
}
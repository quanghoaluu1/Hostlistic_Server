using System.ComponentModel.DataAnnotations.Schema;
using Common;
using StreamingService_Domain.Enums;

namespace StreamingService_Domain.Entities;

public class StreamParticipant : BaseClass
{
    public Guid StreamRoomId { get; set; }
    public Guid UserId { get; set; }
    
    public ParticipantRole Role { get; set; }
    public string? LiveKitIdentity { get; set; }
    
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsCurrentlyConnected { get; set; }
    public bool IsBanned { get; set; }

    [ForeignKey(nameof(StreamRoomId))]
    public virtual StreamRoom StreamRoom { get; set; } = null!;
}
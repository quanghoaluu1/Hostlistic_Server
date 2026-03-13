using System.ComponentModel.DataAnnotations.Schema;
using Common;
using StreamingService_Domain.Enums;

namespace StreamingService_Domain.Entities;

public class StreamRecording : BaseClass
{
    public Guid StreamRoomId { get; set; }
    public string FileName { get; set; } = null!;
    public string? StorageUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public TimeSpan Duration { get; set; }
    public RecordingStatus Status { get; set; }
    public string? EgressId { get; set; } // Livekit egress id

    [ForeignKey(nameof(StreamRoomId))]
    public virtual StreamRoom StreamRoom { get; set; } = null!;
}
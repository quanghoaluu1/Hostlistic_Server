using Common;
using StreamingService_Domain.Enums;

namespace StreamingService_Domain.Entities;

public class StreamRoom : BaseClass
{
    public Guid EventId { get; set; }
    public Guid? SessionId { get; set; }
    
    public string LiveKitRoomName { get; set; } = null!;
    public string LiveKitRoomSid { get; set; } = null!;//Sid tra ve tu livekit
    
    public StreamRoomStatus Status { get; set; }
    public DateTime? ScheduledStartAt { get; set; }
    public DateTime? ActualStartAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    public int MaxParticipants { get; set; }
    public int IsRecordEnabled { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public IEnumerable<StreamParticipant> Participants { get; set; } = new List<StreamParticipant>();
    public IEnumerable<StreamRecording> Recordings { get; set; } = new List<StreamRecording>();
}
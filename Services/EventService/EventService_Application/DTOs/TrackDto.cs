namespace EventService_Application.DTOs;

public class TrackDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int SessionCount { get; set; }  // computed — giúp UI biết track nào trống
    public DateTime CreatedAt { get; set; }
}

public class CreateTrackRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ColorHex { get; set; } = "#6366F1";
}

public class UpdateTrackRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public int? SortOrder { get; set; }
}
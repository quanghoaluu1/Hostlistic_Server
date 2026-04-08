namespace AIService_Application.DTOs.Requests;

public class LineupDetailDto
{
    public List<LineupTalentDto> EventWideTalents { get; set; } = [];
    public List<LineupSessionDto> SessionTalents { get; set; } = [];
    public int TotalTalents { get; set; }
}

public class LineupTalentDto
{
    public Guid TalentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Organization { get; set; }
}

public class LineupSessionDto
{
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTime? SessionStartTime { get; set; }
    public DateTime? SessionEndTime { get; set; }
    public string? TrackName { get; set; }
    public string? TrackColorHex { get; set; }
    public List<LineupTalentDto> Talents { get; set; } = [];
}
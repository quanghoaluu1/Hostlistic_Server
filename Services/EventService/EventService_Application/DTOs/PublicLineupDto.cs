namespace EventService_Application.DTOs;

public class PublicLineupResponse
{
    public List<PublicTalentDto> EventWideTalents { get; set; } = [];
    public List<PublicSessionLineupDto> SessionTalents { get; set; } = [];
    public int TotalTalents { get; set; }
}

public class PublicTalentDto
{
    public Guid TalentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Organization { get; set; }
}

public class PublicSessionLineupDto
{
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTime? SessionStartTime { get; set; }
    public DateTime? SessionEndTime { get; set; }
    public string? TrackName { get; set; }
    public string? TrackColorHex { get; set; }
    public List<PublicTalentDto> Talents { get; set; } = [];
}

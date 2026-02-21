namespace EventService_Application.DTOs
{
    public class LineupDto
    {
        public Guid Id { get; set; }
        public TalentDto Talent { get; set; }
        public Guid? SessionId { get; set; }
        public Guid EventId { get; set; }
    }
    public class CreateLineupsRequest
    {
        public Guid EventId { get; set; }
        public Guid? SessionId { get; set; }
        public List<Guid> TalentIds { get; set; } = new();
    }
    public class BatchLineupResultDto
    {
        public List<LineupDto> Created { get; set; } = new();
        public List<Guid> SkippedTalentIds { get; set; } = new();
    }
}

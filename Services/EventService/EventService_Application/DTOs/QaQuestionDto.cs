using EventService_Domain.Enums;

namespace EventService_Application.DTOs
{
    public class QaQuestionDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QaStatus Status { get; set; }
        public int UpVotes { get; set; }
        public int DurationInSecond { get; set; }
        public TimeSpan AskedAt { get; set; }
    }

    public class CreateQaQuestionDto
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
    }

    public class QaVoteDto
    {
        public Guid UserId { get; set; }
        public Guid QaQuestionId { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }
}

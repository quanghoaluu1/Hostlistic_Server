using EventService_Domain.Entities;
using EventService_Domain.Enums;

namespace EventService_Application.DTOs
{
    public class PollDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<PollOption> Options { get; set; } = [];
        public List<int> CorrectAnswers { get; set; } = [];
        public PollType Type { get; set; }
        public bool IsPrivate { get; set; }
        public int? DurationInSecond { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreatePollRequest
    {
        public Guid SessionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<PollOption> Options { get; set; } = [];
        public List<int> CorrectAnswers { get; set; } = [];
        public PollType Type { get; set; }
        public bool IsPrivate { get; set; }
        public int? DurationInSecond { get; set; }
    }

    public class UpdatePollRequest
    {
        public string? Question { get; set; }
        public List<PollOption>? Options { get; set; }
        public List<int>? CorrectAnswers { get; set; }
        public PollType? Type { get; set; }
        public bool IsPrivate { get; set; }
        public int? DurationInSecond { get; set; }
    }
}

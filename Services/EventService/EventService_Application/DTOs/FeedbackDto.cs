namespace EventService_Application.DTOs
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class UpdateFeedbackDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}

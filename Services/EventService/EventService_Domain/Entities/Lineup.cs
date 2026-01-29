namespace EventService_Domain.Entities;

public class Lineup
{
    public Guid Id { get; set; }
    public Guid TalentId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid EventId { get; set; }
}
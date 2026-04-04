using System.ComponentModel.DataAnnotations.Schema;

namespace EventService_Domain.Entities;

public class EventDay
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public int DayNumber { get; set; }          // 1-based: Day 1, Day 2, Day 3
    public DateOnly Date { get; set; }           // The calendar date for this day
    public string? Title { get; set; }           // e.g. "Opening & Keynotes"
    public string? Theme { get; set; }           // e.g. "AI & Innovation"
    public string? Description { get; set; }

    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
}

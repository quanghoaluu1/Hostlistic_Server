    using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Common;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class Event : BaseClass
{
    public Guid OrganizerId { get; set; }
    public string? Title { get; set; } = string.Empty;
    public RichTextContent? Description { get; set; }
    public EventMode? EventMode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; } = string.Empty;
    public Guid? EventTypeId { get; set; }
    public string? CoverImageUrl { get; set; } = string.Empty;
    public string? CoverImagePublicId { get; set; } = string.Empty;
    public int? TotalCapacity { get; set; }
    public bool? IsPublic { get; set; } = false;
    public EventStatus EventStatus { get; set; } = EventStatus.Draft;
    public AgendaMode AgendaMode { get; set; } = AgendaMode.Auto;
    
    public string? TimeZoneId { get; set; }

    public string? PostponementReason { get; set; }
    public DateTime? OriginalStartDate { get; set; }
    public DateTime? OriginalEndDate { get; set; }

    // Navigation properties to parent
    [ForeignKey("EventTypeId")]
    public virtual EventType? EventType { get; set; }
    
    
    // Navigation properties to children
    public ICollection<EventTeamMember> EventTeamMembers { get; set; } = new List<EventTeamMember>();
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<Lineup> Lineups { get; set; } = new List<Lineup>();
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    public ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Venue> Venues { get; set; } = [];
    public ICollection<EventDay> EventDays { get; set; } = new List<EventDay>();
    public ICollection<SurveyForm> SurveyForms { get; set; } = new List<SurveyForm>();
    
    public void PromoteToCustomAgenda()
    {
        if (AgendaMode == AgendaMode.Auto)
            AgendaMode = AgendaMode.Custom;
    }

    public ApiResponse<bool> Postpone(DateTime currentUtcTime, DateTime? newStartTime, DateTime? newEndTime, string reason)
    {
        var allowedStatuses = new[] { EventStatus.Published };
        if (!allowedStatuses.Contains(EventStatus))
            return ApiResponse<bool>.Fail(400,
                $"Cannot postpone event with status '{EventStatus}'. Only Published events can be postponed.");

        if (StartDate.HasValue && (StartDate.Value - currentUtcTime).TotalHours < 24)
            return ApiResponse<bool>.Fail(400,
                "Cannot postpone event less than 24 hours before its scheduled start time.");

        if (newStartTime.HasValue && newEndTime.HasValue && newEndTime <= newStartTime)
            return ApiResponse<bool>.Fail(400, "New end time must be after new start time.");

        OriginalStartDate = StartDate;
        OriginalEndDate = EndDate;
        PostponementReason = reason;
        EventStatus = EventStatus.Postponed;
        StartDate = newStartTime;
        EndDate = newEndTime;
        UpdatedAt = currentUtcTime;

        return ApiResponse<bool>.Success(200, "Event postponed successfully.", true);
    }
}

public class RichTextContent
{
    public string Type { get; set; } // "doc"
    public JsonElement Content { get; set; } // Các node nội dung
}


using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/days")]
[Authorize]
public class EventDayController(IEventDayService eventDayService) : ControllerBase
{
    /// <summary>
    /// List all days for an event.
    /// Public — attendees and organizers need this for schedule grouping.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetDays(Guid eventId)
    {
        var result = await eventDayService.GetByEventIdAsync(eventId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get a single event day by ID.
    /// </summary>
    [HttpGet("{dayId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDay(Guid eventId, Guid dayId)
    {
        var result = await eventDayService.GetByIdAsync(eventId, dayId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Auto-generate EventDay entries from the event's StartDate to EndDate.
    /// Fails with 409 if days have already been generated.
    /// Requires: authenticated organizer/team member.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateDays(Guid eventId, [FromBody] GenerateEventDaysRequest request)
    {
        var result = await eventDayService.GenerateDaysAsync(eventId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Manually create a single event day for a specific date.
    /// Requires: authenticated organizer/team member.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDay(Guid eventId, [FromBody] CreateEventDayRequest request)
    {
        var result = await eventDayService.CreateAsync(eventId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update metadata (title, theme, description) of an event day.
    /// Date and DayNumber are immutable after creation.
    /// Requires: authenticated organizer/team member.
    /// </summary>
    [HttpPut("{dayId:guid}")]
    public async Task<IActionResult> UpdateDay(Guid eventId, Guid dayId, [FromBody] UpdateEventDayRequest request)
    {
        var result = await eventDayService.UpdateAsync(eventId, dayId, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete an event day.
    /// Requires: authenticated organizer/team member.
    /// </summary>
    [HttpDelete("{dayId:guid}")]
    public async Task<IActionResult> DeleteDay(Guid eventId, Guid dayId)
    {
        var result = await eventDayService.DeleteAsync(eventId, dayId);
        return StatusCode(result.StatusCode, result);
    }
}

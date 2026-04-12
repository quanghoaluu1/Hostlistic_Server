using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService eventService, IPhotoService photoService, IEventLifecycleService lifecycleService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateEventAsync([FromBody] EventRequestDto dto)
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await eventService.CreateEventAsync(dto, organizerId);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllEventsAsync([FromQuery] BaseQueryParams request)
    {
        var result = await eventService.GetAllEventsAsync(request);
        return Ok(result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPublicEventsAsync([FromQuery] PublicEventQueryParams queryParams)
    {
        var result = await eventService.GetPublicEventsAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{eventId:guid}")]
    public async Task<IActionResult> GetEventByIdAsync(Guid eventId)
    {
        var result = await eventService.GetEventByIdAsync(eventId);
        return Ok(result);
    }

    [HttpPatch("cover-image/{eventId:guid}")]
    public async Task<IActionResult> SetEventCover(Guid eventId, IFormFile file)
    {
        var eventEntity = await eventService.GetEventByIdAsync(eventId);
        if (eventEntity.Data == null) return NotFound(eventEntity);
        var result = await photoService.UploadPhotoAsync(file, "cover-images/");
        if (result.Error != null) return BadRequest(result.Error);
        var imageUrl = result.SecureUrl.AbsoluteUri;
        var publicId = result.PublicId;
        var updateCoverImage = new EventRequestDto()
        {
            CoverImageUrl = imageUrl,
        };
        var updateResult = await eventService.UpdateEventAsync(eventId, updateCoverImage, publicId);
        return Ok(updateResult);
    }

    [HttpPatch("{eventId:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid eventId, EventRequestDto dto)
    {
        var eventEntity = await eventService.GetEventByIdAsync(eventId);
        if (eventEntity.Data == null) return NotFound(eventEntity);
        var result = await eventService.UpdateEventAsync(eventId, dto, null);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMyEvents([FromQuery] MyEventQueryParams queryParams)
    {
        var userId = GetCurrentUserId();
        var result = await eventService.GetMyEventAsync(userId, queryParams);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
    [HttpPatch("{eventId:guid}/agenda-mode")]
    public async Task<IActionResult> SetAgendaMode(
        Guid eventId)
    {
        var result = await eventService.ToggleAgendaModeAsync(eventId);
        return StatusCode(result.StatusCode, result);
    }


    [HttpGet("{eventId:guid}/stream-auth")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyStreamAccess(Guid eventId, [FromQuery] Guid userId)
    {
        var result = await eventService.VerifyStreamAccessAsync(eventId, userId);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDashboard([FromQuery] int? year, [FromQuery] int? month)
    {
        var result = await eventService.GetEventDashboardAsync(year, month);
        return Ok(result);
    }

    [HttpPut("status/{eventId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEventStatus(Guid eventId)
    {
        var result = await eventService.UpdateEventStatus(eventId);
        return Ok(result);
    }

    [HttpPatch("{eventId:guid}/start")]
    public async Task<IActionResult> StartEvent(Guid eventId)
    {
        var result = await lifecycleService.StartEventAsync(eventId, GetCurrentUserId());
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{eventId:guid}/complete")]
    public async Task<IActionResult> CompleteEvent(Guid eventId)
    {
        var result = await lifecycleService.CompleteEventAsync(eventId, GetCurrentUserId());
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{eventId:guid}/cancel")]
    public async Task<IActionResult> CancelEvent(Guid eventId, [FromBody] CancelEventRequest request)
    {
        var result = await lifecycleService.CancelEventAsync(eventId, GetCurrentUserId(), request.Reason);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}

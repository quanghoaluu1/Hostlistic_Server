using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService eventService, IPhotoService photoService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateEventAsync([FromBody] EventRequestDto dto)
    {
        var result = await eventService.CreateEventAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEventsAsync()
    {
        var result = await eventService.GetAllEventsAsync();
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
        var result = await photoService.UploadPhotoAsync(file);
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
}
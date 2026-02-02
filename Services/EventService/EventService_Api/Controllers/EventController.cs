using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService eventService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateEventAsync([FromBody] CreateEventDto dto)
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
}
using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")]
public class EventTypeController(IEventTypeService eventTypeService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventTypes([FromQuery] BaseQueryParams request) => Ok(await eventTypeService.GetAllEventTypesAsync(request));

    [HttpGet("{eventTypeId:guid}")]
    public async Task<IActionResult> GetEventType(Guid eventTypeId) => Ok(await eventTypeService.GetEventTypeByIdAsync(eventTypeId));

    [HttpPost]
    public async Task<IActionResult> CreateEventType([FromBody] CreateEventTypeDto dto)
    {
        try
        {
            var result = await eventTypeService.CreateEventTypeAsync(dto);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{eventTypeId:guid}")]
    public async Task<IActionResult> UpdateEventType(Guid eventTypeId, [FromBody] UpdateEventTypeDto dto)
    {
        try
        {
            var result = await eventTypeService.UpdateEventTypeAsync(eventTypeId, dto);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }

}
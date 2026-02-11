using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class TicketTypesController : ControllerBase
{
    private readonly ITicketTypeService _ticketTypeService;

    public TicketTypesController(ITicketTypeService ticketTypeService)
    {
        _ticketTypeService = ticketTypeService;
    }

    [HttpGet("{ticketTypeId:guid}")]
    public async Task<IActionResult> GetTicketTypeById(Guid ticketTypeId)
    {
        var result = await _ticketTypeService.GetTicketTypeByIdAsync(ticketTypeId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetTicketTypesByEventId(Guid eventId)
    {
        var result = await _ticketTypeService.GetTicketTypesByEventIdAsync(eventId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("session/{sessionId:guid}")]
    public async Task<IActionResult> GetTicketTypesBySessionId(Guid sessionId)
    {
        var result = await _ticketTypeService.GetTicketTypesBySessionIdAsync(sessionId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicketType([FromBody] CreateTicketTypeRequest request)
    {
        var result = await _ticketTypeService.CreateTicketTypeAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{ticketTypeId:guid}")]
    public async Task<IActionResult> UpdateTicketType(Guid ticketTypeId, [FromBody] UpdateTicketTypeRequest request)
    {
        var result = await _ticketTypeService.UpdateTicketTypeAsync(ticketTypeId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{ticketTypeId:guid}")]
    public async Task<IActionResult> DeleteTicketType(Guid ticketTypeId)
    {
        var result = await _ticketTypeService.DeleteTicketTypeAsync(ticketTypeId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}

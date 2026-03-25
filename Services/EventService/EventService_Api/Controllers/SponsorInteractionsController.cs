using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SponsorInteractionsController(ISponsorInteractionService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSponsorInteractionDto dto)
    {
        var result = await service.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("by-sponsor/{sponsorId:guid}")]
    public async Task<IActionResult> GetBySponsor(Guid sponsorId, [FromQuery] BaseQueryParams request)
    {
        var result = await service.GetBySponsorIdAsync(sponsorId, request);
        return Ok(result);
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] BaseQueryParams request)
    {
        var result = await service.GetByUserIdAsync(userId, request);
        return Ok(result);
    }
}

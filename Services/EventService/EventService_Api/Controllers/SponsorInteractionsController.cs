using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    public async Task<IActionResult> GetBySponsor(Guid sponsorId)
    {
        var result = await service.GetBySponsorIdAsync(sponsorId);
        return Ok(result);
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var result = await service.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost("track")]
    [Authorize]
    public async Task<IActionResult> TrackInteraction([FromBody] TrackInteractionRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await service.TrackInteractionAsync(request.SponsorId, userId, request.InteractionType);
        return Ok(new { message = "Interaction tracked." });
    }

    [HttpGet("{sponsorId:guid}/stats")]
    [Authorize]
    public async Task<IActionResult> GetStats(Guid sponsorId)
    {
        var result = await service.GetInteractionStatsAsync(sponsorId);
        return Ok(result);
    }
}

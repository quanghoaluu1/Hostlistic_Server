using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class TracksController : ControllerBase
{
    private readonly ITrackService _trackService;

    public TracksController(ITrackService trackService)
    {
        _trackService = trackService;
    }

    [HttpGet("{trackId:guid}")]
    public async Task<IActionResult> GetTrackById(Guid trackId)
    {
        var result = await _trackService.GetTrackByIdAsync(trackId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetTracksByEventId(Guid eventId)
    {
        var result = await _trackService.GetTracksByEventIdAsync(eventId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTrack([FromBody] CreateTrackRequest request)
    {
        var result = await _trackService.CreateTrackAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{trackId:guid}")]
    public async Task<IActionResult> UpdateTrack(Guid trackId, [FromBody] UpdateTrackRequest request)
    {
        var result = await _trackService.UpdateTrackAsync(trackId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{trackId:guid}")]
    public async Task<IActionResult> DeleteTrack(Guid trackId)
    {
        var result = await _trackService.DeleteTrackAsync(trackId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
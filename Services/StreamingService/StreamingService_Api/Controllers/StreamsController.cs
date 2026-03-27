using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using StreamingService_Application.UseCases.Streams.Commands.CreateStreamRoom;
using StreamingService_Application.UseCases.Streams.Commands.EndStreamRoom;
using StreamingService_Application.UseCases.Streams.Queries.GetStreamToken;
using StreamingService_Domain.Entities;
using StreamingService_Domain.Enums;
using StreamingService_Application.Interfaces;
using StreamingService_Api.Hubs;

namespace StreamingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStreamingServiceDbContext _dbContext;
    private readonly IHubContext<StreamingHub> _hubContext;

    public StreamsController(IMediator mediator, IStreamingServiceDbContext dbContext, IHubContext<StreamingHub> hubContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    [HttpPost("rooms")]
    public async Task<IActionResult> CreateStreamRoom([FromBody] CreateStreamRoomCommand command)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized("Missing or invalid user claim.");

        try
        {
            var roomId = await _mediator.Send(command with { CreatedBy = userId });
            await _hubContext.Clients.Group(StreamingHub.BuildEventGroup(command.EventId.ToString()))
                .SendAsync("StreamStateChanged", new
                {
                    EventId = command.EventId,
                    RoomId = roomId,
                    IsLive = true
                });
            return Ok(new { RoomId = roomId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("rooms/{roomId}/token")]
    public async Task<IActionResult> GetStreamToken(Guid roomId, [FromQuery] string identity, [FromQuery] ParticipantRole role = ParticipantRole.Attendee)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized("Missing or invalid user claim.");

        try
        {
            var query = new GetStreamTokenQuery(roomId, userId, identity, role);
            var token = await _mediator.Send(query);
            
            return Ok(new { Token = token });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("events/{eventId}/active-room")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveRoom(Guid eventId)
    {
        var room = await _dbContext.StreamRooms
            .Where(r => r.EventId == eventId && r.Status != StreamRoomStatus.Ended)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                RoomId = r.Id,
                Status = r.Status.ToString(),
                r.CreatedAt,
                r.ActualStartAt,
                r.LiveKitRoomName
            })
            .FirstOrDefaultAsync();

        if (room == null)
        {
            return Ok(new
            {
                RoomId = (Guid?)null,
                Status = "None",
                CreatedAt = (DateTime?)null,
                ActualStartAt = (DateTime?)null,
                LiveKitRoomName = string.Empty
            });
        }

        return Ok(room);
    }

    [HttpPost("rooms/{roomId}/end")]
    public async Task<IActionResult> EndStreamRoom(Guid roomId)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized("Missing or invalid user claim.");

        var room = await _dbContext.StreamRooms.FirstOrDefaultAsync(r => r.Id == roomId);
        var command = new EndStreamRoomCommand(roomId, userId);
        await _mediator.Send(command);

        if (room != null)
        {
            await _hubContext.Clients.Group(StreamingHub.BuildEventGroup(room.EventId.ToString()))
                .SendAsync("StreamStateChanged", new
                {
                    EventId = room.EventId,
                    RoomId = roomId,
                    IsLive = false
                });
        }
        
        return NoContent();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(rawUserId, out userId);
    }
}

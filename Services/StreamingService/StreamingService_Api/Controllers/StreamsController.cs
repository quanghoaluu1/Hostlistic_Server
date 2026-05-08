using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
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
    private readonly IEventServiceClient _eventServiceClient;
    private readonly IBookingServiceClient _bookingServiceClient;
    private readonly IGuestStreamAccessService _guestStreamAccessService;
    private readonly ITokenGenerator _tokenGenerator;

    public StreamsController(
        IMediator mediator,
        IStreamingServiceDbContext dbContext,
        IHubContext<StreamingHub> hubContext,
        IEventServiceClient eventServiceClient,
        IBookingServiceClient bookingServiceClient,
        IGuestStreamAccessService guestStreamAccessService,
        ITokenGenerator tokenGenerator)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _eventServiceClient = eventServiceClient;
        _bookingServiceClient = bookingServiceClient;
        _guestStreamAccessService = guestStreamAccessService;
        _tokenGenerator = tokenGenerator;
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
                    TrackId = command.TrackId,
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
    public async Task<IActionResult> GetActiveRoom(Guid eventId, [FromQuery] Guid? trackId = null)
    {
        var room = await _dbContext.StreamRooms
            .Where(r =>
                r.EventId == eventId &&
                (!trackId.HasValue || r.TrackId == trackId))
            .Select(r => new
            {
                Room = r,
                HasConnectedHost = _dbContext.StreamParticipants.Any(p =>
                    p.StreamRoomId == r.Id &&
                    p.IsCurrentlyConnected &&
                    (p.Role == ParticipantRole.Organizer || p.Role == ParticipantRole.CoOrganizer)),
                HasHostHistory = _dbContext.StreamParticipants.Any(p =>
                    p.StreamRoomId == r.Id &&
                    (p.Role == ParticipantRole.Organizer || p.Role == ParticipantRole.CoOrganizer) &&
                    (p.JoinedAt != null || !string.IsNullOrWhiteSpace(p.LiveKitIdentity)))
            })
            .Where(x =>
                x.Room.Status == StreamRoomStatus.Live ||
                (x.Room.Status != StreamRoomStatus.Ended && (x.HasConnectedHost || x.HasHostHistory)))
            .OrderByDescending(r => r.Room.CreatedAt)
            .Select(r => new
            {
                RoomId = r.Room.Id,
                r.Room.TrackId,
                r.Room.SessionId,
                r.Room.CreatedBy,
                Status = (r.Room.Status == StreamRoomStatus.Live ||
                          (r.Room.Status != StreamRoomStatus.Ended && (r.HasConnectedHost || r.HasHostHistory)))
                    ? StreamRoomStatus.Live.ToString()
                    : r.Room.Status.ToString(),
                r.Room.CreatedAt,
                r.Room.ActualStartAt,
                r.Room.LiveKitRoomName
            })
            .FirstOrDefaultAsync();

        if (room == null)
        {
            return Ok(new
            {
                RoomId = (Guid?)null,
                TrackId = (Guid?)null,
                SessionId = (Guid?)null,
                CreatedBy = (Guid?)null,
                Status = "None",
                CreatedAt = (DateTime?)null,
                ActualStartAt = (DateTime?)null,
                LiveKitRoomName = string.Empty
            });
        }

        return Ok(room);
    }

    [HttpGet("events/{eventId}/active-rooms")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveRooms(Guid eventId)
    {
        var rooms = await _dbContext.StreamRooms
            .Where(r => r.EventId == eventId)
            .Select(r => new
            {
                Room = r,
                HasConnectedHost = _dbContext.StreamParticipants.Any(p =>
                    p.StreamRoomId == r.Id &&
                    p.IsCurrentlyConnected &&
                    (p.Role == ParticipantRole.Organizer || p.Role == ParticipantRole.CoOrganizer)),
                HasHostHistory = _dbContext.StreamParticipants.Any(p =>
                    p.StreamRoomId == r.Id &&
                    (p.Role == ParticipantRole.Organizer || p.Role == ParticipantRole.CoOrganizer) &&
                    (p.JoinedAt != null || !string.IsNullOrWhiteSpace(p.LiveKitIdentity)))
            })
            .Where(x =>
                x.Room.Status == StreamRoomStatus.Live ||
                (x.Room.Status != StreamRoomStatus.Ended && (x.HasConnectedHost || x.HasHostHistory)))
            .OrderByDescending(x => x.Room.CreatedAt)
            .Select(x => new
            {
                RoomId = x.Room.Id,
                x.Room.TrackId,
                x.Room.SessionId,
                x.Room.CreatedBy,
                Status = (x.Room.Status == StreamRoomStatus.Live ||
                          (x.Room.Status != StreamRoomStatus.Ended && (x.HasConnectedHost || x.HasHostHistory)))
                    ? StreamRoomStatus.Live.ToString()
                    : x.Room.Status.ToString(),
                x.Room.CreatedAt,
                x.Room.ActualStartAt,
                x.Room.LiveKitRoomName
            })
            .ToListAsync();

        return Ok(rooms);
    }

    [HttpPost("events/{eventId}/guest-access")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateGuestAccess(Guid eventId, [FromBody] GuestStreamAccessRequest request)
    {
        var clientKey = BuildClientKey();
        var attemptStatus = _guestStreamAccessService.GetAttemptStatus(eventId, clientKey);
        if (attemptStatus.IsBlocked)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "Too many invalid ticket code attempts. Please try again later.",
                attemptStatus.BlockedUntilUtc,
                attemptStatus.FailedAttempts,
                attemptStatus.RemainingAttempts
            });
        }

        var room = await _dbContext.StreamRooms
            .AsNoTracking()
            .Where(r =>
                r.EventId == eventId &&
                (!request.TrackId.HasValue || r.TrackId == request.TrackId.Value))
            .Select(r => new
            {
                Room = r,
                HasConnectedHost = _dbContext.StreamParticipants.Any(p =>
                    p.StreamRoomId == r.Id &&
                    p.IsCurrentlyConnected &&
                    (p.Role == ParticipantRole.Organizer || p.Role == ParticipantRole.CoOrganizer)),
                HasHostHistory = _dbContext.StreamParticipants.Any(p =>
                    p.StreamRoomId == r.Id &&
                    (p.Role == ParticipantRole.Organizer || p.Role == ParticipantRole.CoOrganizer) &&
                    (p.JoinedAt != null || !string.IsNullOrWhiteSpace(p.LiveKitIdentity)))
            })
            .Where(x =>
                x.Room.Status == StreamRoomStatus.Live ||
                (x.Room.Status != StreamRoomStatus.Ended && (x.HasConnectedHost || x.HasHostHistory)))
            .OrderByDescending(x => x.Room.CreatedAt)
            .Select(x => x.Room)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (room == null)
        {
            return BadRequest(new
            {
                message = request.TrackId.HasValue
                    ? "There is no live stream active for the selected track right now."
                    : "There is no live stream active for this event right now."
            });
        }

        var normalizedTicketCode = NormalizeTicketCode(request.TicketCode);
        var ticket = await _bookingServiceClient.ValidateGuestLiveTicketAsync(eventId, normalizedTicketCode, HttpContext.RequestAborted);
        if (ticket == null)
        {
            var failedAttempt = _guestStreamAccessService.RegisterFailedAttempt(eventId, clientKey);
            var statusCode = failedAttempt.IsBlocked
                ? StatusCodes.Status429TooManyRequests
                : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new
            {
                message = failedAttempt.IsBlocked
                    ? "Too many invalid ticket code attempts. Please try again in 10 minutes."
                    : "Ticket code is invalid for this live event.",
                failedAttempt.BlockedUntilUtc,
                failedAttempt.FailedAttempts,
                failedAttempt.RemainingAttempts
            });
        }

        var ticketTypeAccess = await _eventServiceClient.GetTicketTypeStreamingAccessAsync(ticket.TicketTypeId, HttpContext.RequestAborted);
        if (ticketTypeAccess != null
            && room.TrackId.HasValue
            && ticketTypeAccess.AllowedTrackIds.Count > 0
            && !ticketTypeAccess.AllowedTrackIds.Contains(room.TrackId.Value))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "This ticket type is not allowed to access the selected track."
            });
        }

        if (_guestStreamAccessService.TryGetActiveSession(ticket.TicketId, out var activeSession) && activeSession != null)
        {
            return Conflict(new
            {
                message = "This ticket is already being used in another live session.",
                activeSessionId = activeSession.SessionId,
                activeSession.LastSeenAtUtc,
                activeSession.ExpiresAtUtc
            });
        }

        _guestStreamAccessService.ResetAttempts(eventId, clientKey);

        var guestSession = _guestStreamAccessService.CreateOrReplaceSession(eventId, room.Id, ticket, request.HolderName);
        var token = GenerateGuestToken(room.LiveKitRoomName, guestSession.Identity);

        return Ok(new
        {
            roomId = room.Id,
            token,
            identity = guestSession.Identity,
            guestSessionId = guestSession.SessionId,
            expiresAtUtc = guestSession.ExpiresAtUtc,
            ticketCode = ticket.TicketCode,
            holderName = guestSession.HolderName ?? "Guest",
            roomStatus = room.Status.ToString()
        });
    }

    [HttpPost("guest-sessions/{sessionId:guid}/heartbeat")]
    [AllowAnonymous]
    public IActionResult HeartbeatGuestSession(Guid sessionId)
    {
        if (!_guestStreamAccessService.TouchSession(sessionId, out var session) || session == null)
            return NotFound(new { message = "Guest live session not found or expired." });

        return Ok(new
        {
            sessionId = session.SessionId,
            session.ExpiresAtUtc,
            session.LastSeenAtUtc
        });
    }

    [HttpPost("guest-sessions/{sessionId:guid}/release")]
    [AllowAnonymous]
    public IActionResult ReleaseGuestSession(Guid sessionId)
    {
        _guestStreamAccessService.ReleaseSession(sessionId);
        return NoContent();
    }

    [HttpGet("events/{eventId}/recordings")]
    public async Task<IActionResult> GetEventRecordings(Guid eventId, [FromQuery] Guid? trackId = null)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized("Missing or invalid user claim.");

        var access = await _eventServiceClient.VerifyStreamAccessAsync(eventId, userId, null, HttpContext.RequestAborted);
        if (!access.IsAllowed)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = access.ErrorMessage ?? "You are not allowed to view recordings for this event." });

        var recordings = await _dbContext.StreamRecordings
            .AsNoTracking()
            .Where(r =>
                r.StreamRoom.EventId == eventId &&
                r.Status == RecordingStatus.Ready &&
                (!trackId.HasValue || r.StreamRoom.TrackId == trackId.Value))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(recordings.Select(r => new
        {
            id = r.Id,
            streamRoomId = r.StreamRoomId,
            fileName = r.FileName,
            playbackUrl = BuildPublicRecordingUrl(r.StorageUrl),
            fileSizeBytes = r.FileSizeBytes,
            durationSeconds = r.Duration.TotalSeconds,
            createdAt = r.CreatedAt,
            updatedAt = r.UpdatedAt
        }));
    }

    [HttpPost("rooms/{roomId}/recordings/upload")]
    public IActionResult UploadRecording(Guid roomId)
    {
        if (!TryGetCurrentUserId(out _))
            return Unauthorized("Missing or invalid user claim.");

        return StatusCode(StatusCodes.Status410Gone, new
        {
            roomId,
            message = "Browser-based recording upload is no longer supported. Recording files must be ingested by the configured server-side recording pipeline."
        });
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
                    TrackId = room.TrackId,
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

    private string? BuildPublicRecordingUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.ToString();

        var request = HttpContext.Request;
        if (!request.Host.HasValue)
            return value;

        var normalizedPath = value.StartsWith('/') ? value : $"/{value}";
        return $"{request.Scheme}://{request.Host}{normalizedPath}";
    }

    private string BuildClientKey()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        return $"{ipAddress}:{userAgent}";
    }

    private string NormalizeTicketCode(string? value)
    {
        var compact = Regex.Replace(value ?? string.Empty, @"\s+", string.Empty);
        return compact.Trim().ToUpperInvariant();
    }

    private string GenerateGuestToken(string roomName, string identity)
    {
        return _tokenGenerator.GenerateLiveKitToken(roomName, identity, ParticipantRole.Attendee);
    }
}

public class GuestStreamAccessRequest
{
    public string TicketCode { get; set; } = string.Empty;
    public string? HolderName { get; set; }
    public Guid? TrackId { get; set; }
}

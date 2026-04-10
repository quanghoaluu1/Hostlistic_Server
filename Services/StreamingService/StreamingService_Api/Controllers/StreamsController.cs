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
    private readonly IEventServiceClient _eventServiceClient;

    public StreamsController(
        IMediator mediator,
        IStreamingServiceDbContext dbContext,
        IHubContext<StreamingHub> hubContext,
        IEventServiceClient eventServiceClient)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _hubContext = hubContext;
        _eventServiceClient = eventServiceClient;
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
                Status = "None",
                CreatedAt = (DateTime?)null,
                ActualStartAt = (DateTime?)null,
                LiveKitRoomName = string.Empty
            });
        }

        return Ok(room);
    }

    [HttpGet("events/{eventId}/recordings")]
    public async Task<IActionResult> GetEventRecordings(Guid eventId, [FromQuery] Guid? trackId = null)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized("Missing or invalid user claim.");

        var access = await _eventServiceClient.VerifyStreamAccessAsync(eventId, userId, HttpContext.RequestAborted);
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
    [RequestSizeLimit(long.MaxValue)]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<IActionResult> UploadRecording(
        Guid roomId,
        [FromServices] IRecordingStorageService recordingStorageService,
        [FromForm] IFormFile file,
        [FromForm] double? durationSeconds,
        [FromForm] string? egressId,
        CancellationToken cancellationToken
    )
    {
        if (!TryGetCurrentUserId(out _))
            return Unauthorized("Missing or invalid user claim.");

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Recording file is required." });

        var room = await _dbContext.StreamRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);

        if (room == null)
            return NotFound(new { message = "Stream room not found." });

        if (!room.IsRecordEnabled)
            return BadRequest(new { message = "Recording is disabled for this room." });

        await using var fileStream = file.OpenReadStream();
        var stored = await recordingStorageService.SaveAsync(
            fileStream,
            file.FileName,
            room.EventId,
            roomId,
            cancellationToken
        );

        var recording = await _dbContext.StreamRecordings
            .Where(r => r.StreamRoomId == roomId && r.Status == RecordingStatus.Processing)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (recording == null)
        {
            // Idempotency: if uploader retries the same file for this room, update existing ready row.
            recording = await _dbContext.StreamRecordings
                .Where(r =>
                    r.StreamRoomId == roomId &&
                    r.Status == RecordingStatus.Ready &&
                    r.FileSizeBytes == stored.FileSizeBytes &&
                    r.FileName == stored.FileName)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (recording == null)
        {
            recording = new StreamRecording
            {
                Id = Guid.NewGuid(),
                StreamRoomId = roomId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.StreamRecordings.Add(recording);
        }

        recording.FileName = stored.FileName;
        recording.StorageUrl = BuildPublicRecordingUrl(stored.PlaybackUrl);
        recording.FileSizeBytes = stored.FileSizeBytes;
        recording.Duration = TimeSpan.FromSeconds(Math.Max(0, durationSeconds ?? 0));
        recording.Status = RecordingStatus.Ready;
        recording.EgressId = string.IsNullOrWhiteSpace(egressId) ? null : egressId;
        recording.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients
            .Group(StreamingHub.BuildEventGroup(room.EventId.ToString()))
            .SendAsync("RecordingUploaded", new
            {
                eventId = room.EventId,
                roomId,
                recordingId = recording.Id,
                fileName = recording.FileName,
                playbackUrl = BuildPublicRecordingUrl(recording.StorageUrl),
                fileSizeBytes = recording.FileSizeBytes,
                durationSeconds = recording.Duration.TotalSeconds,
                createdAt = recording.CreatedAt
            }, cancellationToken);

        return Ok(new
        {
            id = recording.Id,
            streamRoomId = recording.StreamRoomId,
            fileName = recording.FileName,
            playbackUrl = BuildPublicRecordingUrl(recording.StorageUrl),
            fileSizeBytes = recording.FileSizeBytes,
            durationSeconds = recording.Duration.TotalSeconds,
            createdAt = recording.CreatedAt
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
}

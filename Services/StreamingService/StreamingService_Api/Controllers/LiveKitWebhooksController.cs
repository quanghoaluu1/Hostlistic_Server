using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Entities;
using StreamingService_Domain.Enums;
using StreamingService_Infrastructure.Settings;

namespace StreamingService_Api.Controllers;

[ApiController]
[Route("api/webhooks/livekit")]
public class LiveKitWebhooksController : ControllerBase
{
    private readonly LiveKitSettings _settings;
    private readonly ILogger<LiveKitWebhooksController> _logger;
    private readonly IStreamingServiceDbContext _dbContext;

    public LiveKitWebhooksController(
        IOptions<LiveKitSettings> options,
        ILogger<LiveKitWebhooksController> logger,
        IStreamingServiceDbContext dbContext)
    {
        _settings = options.Value;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
            return Unauthorized("Missing authorization header");

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var isValid = ValidateWebhookSignature(authHeader, body, string.IsNullOrWhiteSpace(_settings.WebhookSecret) ? _settings.ApiSecret : _settings.WebhookSecret);
        if (!isValid)
            return Unauthorized("Invalid signature");

        try
        {
            using var jsonDocument = JsonDocument.Parse(body);
            var root = jsonDocument.RootElement;
            var eventType = root.GetProperty("event").GetString();

            _logger.LogInformation("Received LiveKit Webhook: {EventType}", eventType);

            switch (eventType)
            {
                case "room_started":
                    await HandleRoomStartedAsync(root);
                    break;
                case "room_finished":
                    await HandleRoomFinishedAsync(root);
                    break;
                case "participant_joined":
                    await HandleParticipantJoinedAsync(root);
                    break;
                case "participant_left":
                case "participant_connection_aborted":
                    await HandleParticipantLeftAsync(root);
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing LiveKit webhook");
            return BadRequest();
        }
    }

    private bool ValidateWebhookSignature(string authHeader, string body, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            return false;

        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..].Trim()
            : authHeader.Trim();

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(_settings.ApiKey),
            ValidIssuer = _settings.ApiKey,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var sha256Claim = principal.FindFirst("sha256")?.Value;
            if (string.IsNullOrWhiteSpace(sha256Claim))
                return false;

            var bodyHash = SHA256.HashData(Encoding.UTF8.GetBytes(body));
            var claimHash = Convert.FromBase64String(sha256Claim);

            return CryptographicOperations.FixedTimeEquals(bodyHash, claimHash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate LiveKit webhook signature");
            return false;
        }
    }

    private async Task HandleRoomStartedAsync(JsonElement root)
    {
        if (!TryGetRoomIdentifiers(root, out var roomName, out var roomSid))
            return;

        var streamRoom = _dbContext.StreamRooms.FirstOrDefault(r => r.LiveKitRoomName == roomName);
        if (streamRoom == null)
        {
            _logger.LogWarning("Received room_started for unknown room {RoomName}", roomName);
            return;
        }

        streamRoom.LiveKitRoomSid = roomSid ?? streamRoom.LiveKitRoomSid;
        streamRoom.Status = StreamRoomStatus.Live;
        streamRoom.ActualStartAt ??= DateTime.UtcNow;
        streamRoom.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
    }

    private async Task HandleRoomFinishedAsync(JsonElement root)
    {
        if (!TryGetRoomIdentifiers(root, out var roomName, out var roomSid))
            return;

        var streamRoom = _dbContext.StreamRooms.FirstOrDefault(r => r.LiveKitRoomName == roomName);
        if (streamRoom == null)
        {
            _logger.LogWarning("Received room_finished for unknown room {RoomName}", roomName);
            return;
        }

        streamRoom.LiveKitRoomSid = roomSid ?? streamRoom.LiveKitRoomSid;
        streamRoom.Status = StreamRoomStatus.Ended;
        streamRoom.EndedAt ??= DateTime.UtcNow;
        streamRoom.UpdatedAt = DateTime.UtcNow;

        foreach (var participant in _dbContext.StreamParticipants.Where(p => p.StreamRoomId == streamRoom.Id && p.IsCurrentlyConnected))
        {
            participant.IsCurrentlyConnected = false;
            participant.LeftAt ??= DateTime.UtcNow;
            participant.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
    }

    private async Task HandleParticipantJoinedAsync(JsonElement root)
    {
        if (!TryGetRoomIdentifiers(root, out var roomName, out _) || !TryGetParticipantIdentity(root, out var identity))
            return;

        var streamRoom = _dbContext.StreamRooms.FirstOrDefault(r => r.LiveKitRoomName == roomName);
        if (streamRoom == null)
        {
            _logger.LogWarning("Received participant_joined for unknown room {RoomName}", roomName);
            return;
        }

        var participant = _dbContext.StreamParticipants
            .FirstOrDefault(p => p.StreamRoomId == streamRoom.Id && p.LiveKitIdentity == identity);

        if (participant == null)
        {
            _logger.LogWarning("Received participant_joined for unknown identity {Identity} in room {RoomName}", identity, roomName);
            return;
        }

        participant.JoinedAt ??= DateTime.UtcNow;
        participant.LeftAt = null;
        participant.IsCurrentlyConnected = true;
        participant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
    }

    private async Task HandleParticipantLeftAsync(JsonElement root)
    {
        if (!TryGetRoomIdentifiers(root, out var roomName, out _) || !TryGetParticipantIdentity(root, out var identity))
            return;

        var streamRoom = _dbContext.StreamRooms.FirstOrDefault(r => r.LiveKitRoomName == roomName);
        if (streamRoom == null)
        {
            _logger.LogWarning("Received participant_left for unknown room {RoomName}", roomName);
            return;
        }

        var participant = _dbContext.StreamParticipants
            .FirstOrDefault(p => p.StreamRoomId == streamRoom.Id && p.LiveKitIdentity == identity);

        if (participant == null)
        {
            _logger.LogWarning("Received participant_left for unknown identity {Identity} in room {RoomName}", identity, roomName);
            return;
        }

        participant.IsCurrentlyConnected = false;
        participant.LeftAt ??= DateTime.UtcNow;
        participant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
    }

    private static bool TryGetRoomIdentifiers(JsonElement root, out string? roomName, out string? roomSid)
    {
        roomName = null;
        roomSid = null;

        if (!root.TryGetProperty("room", out var room))
            return false;

        if (room.TryGetProperty("name", out var nameElement))
            roomName = nameElement.GetString();

        if (room.TryGetProperty("sid", out var sidElement))
            roomSid = sidElement.GetString();

        return !string.IsNullOrWhiteSpace(roomName);
    }

    private static bool TryGetParticipantIdentity(JsonElement root, out string? identity)
    {
        identity = null;

        if (!root.TryGetProperty("participant", out var participant))
            return false;

        if (participant.TryGetProperty("identity", out var identityElement))
            identity = identityElement.GetString();

        return !string.IsNullOrWhiteSpace(identity);
    }
}

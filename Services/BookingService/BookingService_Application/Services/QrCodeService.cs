using System.Security.Cryptography;
using System.Text;
using BookingService_Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class QrCodeService(IConfiguration configuration, ILogger<QrCodeService> logger) : IQrCodeService
{
    // Key is shared between QR generation (ticket issuance) and verification (check-in scan).
    // Must match on all instances — store in environment-specific appsettings or secrets manager.
    private readonly byte[] _keyBytes = Encoding.UTF8.GetBytes(
        configuration["QrSecret"] ?? throw new InvalidOperationException("QrSecret is not configured"));

    /// <inheritdoc/>
    public Task<string> GenerateQrPayloadAsync(Guid ticketId, Guid eventId)
    {
        var message = $"{ticketId}:{eventId}";
        var sig = ComputeHmacHex(message);
        return Task.FromResult($"{message}:{sig}");
    }

    /// <inheritdoc/>
    public bool VerifyQrPayload(string qrPayload, out Guid ticketId, out Guid eventId)
    {
        ticketId = Guid.Empty;
        eventId = Guid.Empty;

        // Expected format: {guid}:{guid}:{64-char hex}
        // GUIDs contain hyphens but not colons, so splitting on ':' yields exactly 3 segments.
        var firstColon = qrPayload.IndexOf(':');
        if (firstColon < 0)
        {
            logger.LogWarning("QR verification failed: no colon found in payload (length={Length})", qrPayload.Length);
            return false;
        }

        var lastColon = qrPayload.LastIndexOf(':');
        if (lastColon == firstColon)
        {
            logger.LogWarning("QR verification failed: only one colon found, expected 2 (payload={Payload})", qrPayload);
            return false;
        }

        var part0 = qrPayload[..firstColon];
        var part1 = qrPayload[(firstColon + 1)..lastColon];
        var part2 = qrPayload[(lastColon + 1)..];

        if (!Guid.TryParse(part0, out ticketId))
        {
            logger.LogWarning("QR verification failed: part0 is not a valid GUID (part0={Part0})", part0);
            return false;
        }

        if (!Guid.TryParse(part1, out eventId))
        {
            logger.LogWarning("QR verification failed: part1 is not a valid GUID (part1={Part1})", part1);
            return false;
        }

        var message = $"{part0}:{part1}";
        var expected = ComputeHmacHex(message);

        var match = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(part2));

        if (!match)
        {
            logger.LogWarning(
                "QR verification failed: HMAC mismatch. ticketId={TicketId}, eventId={EventId}, " +
                "received_sig={ReceivedSig}, expected_sig={ExpectedSig}, key_length={KeyLength}",
                ticketId, eventId, part2, expected, _keyBytes.Length);
        }

        return match;
    }

    private string ComputeHmacHex(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var hash = HMACSHA256.HashData(_keyBytes, messageBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

using System.Security.Cryptography;
using System.Text;
using BookingService_Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BookingService_Application.Services;

public class QrCodeService(IConfiguration configuration) : IQrCodeService
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
        if (firstColon < 0) return false;
        var lastColon = qrPayload.LastIndexOf(':');
        if (lastColon == firstColon) return false;

        var part0 = qrPayload[..firstColon];
        var part1 = qrPayload[(firstColon + 1)..lastColon];
        var part2 = qrPayload[(lastColon + 1)..];

        if (!Guid.TryParse(part0, out ticketId)) return false;
        if (!Guid.TryParse(part1, out eventId)) return false;

        var message = $"{part0}:{part1}";
        var expected = ComputeHmacHex(message);

        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(part2));
    }

    private string ComputeHmacHex(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var hash = HMACSHA256.HashData(_keyBytes, messageBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

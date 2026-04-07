namespace BookingService_Application.Interfaces;

public interface IQrCodeService
{
    /// <summary>
    /// Generates the QR payload string: <c>{ticketId}:{eventId}:{hmac-hex}</c>.
    /// This is stored as the ticket's QrCodeUrl and encoded into the QR image shown to the attendee.
    /// </summary>
    Task<string> GenerateQrPayloadAsync(Guid ticketId, Guid eventId);

    /// <summary>
    /// Verifies the HMAC signature of a scanned QR payload and extracts ticketId / eventId.
    /// Returns <c>false</c> if the payload is malformed or the signature does not match.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    bool VerifyQrPayload(string qrPayload, out Guid ticketId, out Guid eventId);
}

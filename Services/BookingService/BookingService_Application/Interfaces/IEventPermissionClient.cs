namespace BookingService_Application.Interfaces;

public interface IEventPermissionClient
{
    Task<bool> HasPermissionAsync(
        Guid eventId, Guid userId, string permissionKey,
        CancellationToken ct = default);
}
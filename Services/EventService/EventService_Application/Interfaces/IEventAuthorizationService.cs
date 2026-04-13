namespace EventService_Application.Interfaces;

public interface IEventAuthorizationService
{
    /// <summary>
    /// Check if a user has a specific permission on an event.
    /// </summary>
    /// <param name="eventId">The event to check against.</param>
    /// <param name="userId">The user requesting access.</param>
    /// <param name="permissionKey">
    ///   One of the constants from <see cref="Domain.Constants.EventPermissions"/>.
    ///   Example: "can_export_data", "can_manage_tickets"
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if user has the permission; false otherwise.</returns>
    Task<bool> HasPermissionAsync(
        Guid eventId,
        Guid userId,
        string permissionKey,
        CancellationToken ct = default);

    /// <summary>
    /// Check if a user has ANY of the specified permissions on an event.
    /// Useful for UI: "show this button if user can do X or Y".
    /// </summary>
    Task<bool> HasAnyPermissionAsync(
        Guid eventId,
        Guid userId,
        IEnumerable<string> permissionKeys,
        CancellationToken ct = default);

    /// <summary>
    /// Check if a user is the event owner (creator/organizer).
    /// Useful when only the owner can perform an action (e.g., delete event, manage team).
    /// </summary>
    Task<bool> IsEventOwnerAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Get all permissions a user has on an event.
    /// Returns empty dictionary if user has no role on the event.
    /// For Organizer: returns all keys set to true.
    /// </summary>
    Task<Dictionary<string, bool>> GetUserPermissionsAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct = default);
}
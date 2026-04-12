using EventService_Application.Interfaces;
using EventService_Domain.Constants;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventService_Application.Services;

public class EventAuthorizationService(
    IEventRepository eventRepository,
    IEventTeamMemberRepository teamMemberRepository,
    ILogger<EventAuthorizationService> logger): IEventAuthorizationService
{
    public async Task<bool> HasPermissionAsync(Guid eventId, Guid userId, string permissionKey, CancellationToken ct = default)
    {
        if (!EventPermissions.AllKeys.Contains(permissionKey))
        {
            
            logger.LogWarning(
                "Invalid permission key '{Key}' requested for event {EventId} by user {UserId}",
                permissionKey, eventId, userId);
            return false;
        }

        var isOwner = await eventRepository.IsOwnerAsync(eventId, userId);
        
        if (isOwner)
        {
            logger.LogDebug(
                "User {UserId} is event owner for {EventId}, permission '{Key}' granted",
                userId, eventId, permissionKey);
            return true;
        }

        var memberQueryable =  teamMemberRepository.GetQueryable();
        var member = await memberQueryable.AsNoTracking().Where(m =>
                m.EventId == eventId && m.UserId == userId && m.Status == EventMemberStatus.Active)
            .Select(m => new { m.Permissions, m.Role })
            .FirstOrDefaultAsync(ct);
        if (member is null)
        {
            logger.LogDebug(
                "User {UserId} is not an active team member of event {EventId}, denied",
                userId, eventId);
            return false;
        }
        
        if (member.Permissions.TryGetValue(permissionKey, out var granted) && granted)
        {
            logger.LogDebug(
                "User {UserId} ({Role}) has permission '{Key}' on event {EventId}",
                userId, member.Role, permissionKey, eventId);
            return true;
        }

        logger.LogDebug(
            "User {UserId} ({Role}) does NOT have permission '{Key}' on event {EventId}",
            userId, member.Role, permissionKey, eventId);
        return false;
    }
    
    /// <inheritdoc />
    public async Task<bool> HasAnyPermissionAsync(
        Guid eventId,
        Guid userId,
        IEnumerable<string> permissionKeys,
        CancellationToken ct = default)
    {
        var isOwner = await eventRepository.IsOwnerAsync(eventId, userId);
        
        if (isOwner) return true;

        var memberQueryable =  teamMemberRepository.GetQueryable();
        var member = await memberQueryable.AsNoTracking().Where(m =>
                m.EventId == eventId && m.UserId == userId && m.Status == EventMemberStatus.Active)
            .Select(m => new { m.Permissions })
            .FirstOrDefaultAsync(ct);

        if (member?.Permissions is null) return false;

        return permissionKeys.Any(key =>
            member.Permissions.TryGetValue(key, out var granted) && granted);
    }
    
    public async Task<bool> IsEventOwnerAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct = default)
    {
        return await eventRepository.IsOwnerAsync(eventId, userId);
    }
    
    public async Task<Dictionary<string, bool>> GetUserPermissionsAsync(
        Guid eventId,
        Guid userId,
        CancellationToken ct = default)
    {
        // ── Organizer: return all permissions as true ──
        var isOwner = await eventRepository.IsOwnerAsync(eventId, userId);

        if (isOwner)
        {
            return EventPermissions.AllKeys
                .ToDictionary(key => key, _ => true);
        }

        // ── Team member: return their stored permissions ──
        var permissions = await teamMemberRepository.GetPermissionsByMemberIdAsync(eventId, userId);

        return permissions ?? new Dictionary<string, bool>();
    }

}
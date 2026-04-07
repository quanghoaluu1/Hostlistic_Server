using Common;
using Common.Messages;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Constants;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Exceptions;
using EventService_Domain.Interfaces;
using MassTransit;

namespace EventService_Application.Services;

public class TeamMemberService(
    IEventTeamMemberRepository memberRepository,
    IEventRepository eventRepository,
    IPublishEndpoint publishEndpoint
) : ITeamMemberService
{
    private const int MaxTeamMembers = 50;

    public async Task<ApiResponse<TeamMemberDto>> InviteAsync(
        Guid eventId,
        Guid currentUserId,
        string? currentUserName,
        InviteTeamMemberRequest request)
    {
        // 1. Verify event exists
        var ev = await eventRepository.GetEventByIdAsync(eventId);
        if (ev is null)
            return ApiResponse<TeamMemberDto>.Fail(404, "Event not found.");

        // 2. Authorization check
        bool isOrganizer = ev.OrganizerId == currentUserId;
        EventTeamMember? currentMember = null;

        if (!isOrganizer)
        {
            currentMember = await memberRepository.GetMemberByUserAndEventAsync(currentUserId, eventId);
            bool canManageTeam = currentMember is { Status: EventMemberStatus.Active } &&
                                 currentMember.Permissions.TryGetValue(EventPermissions.CanManageTeam, out var v) && v;

            if (!canManageTeam)
                return ApiResponse<TeamMemberDto>.Fail(403, "You do not have permission to invite team members.");
        }

        // 3. CoOrganizer cannot invite other CoOrganizers
        if (!isOrganizer && EventPermissions.OrganizerOnlyInviteRoles.Contains(request.Role))
            return ApiResponse<TeamMemberDto>.Fail(403, "Only the event organizer can invite CoOrganizers.");

        // 4. CoOrganizer cannot grant organizer-only permissions (e.g., can_manage_team)
        if (!isOrganizer && request.Permissions is not null)
        {
            var forbidden = request.Permissions
                .Where(p => EventPermissions.OrganizerOnlyPermissions.Contains(p.Key) && p.Value)
                .Select(p => p.Key)
                .ToList();

            if (forbidden.Count > 0)
                return ApiResponse<TeamMemberDto>.Fail(403,
                    $"You cannot grant the following permissions: {string.Join(", ", forbidden)}.");
        }

        // 5. Cannot invite yourself
        if (request.UserId == currentUserId)
            return ApiResponse<TeamMemberDto>.Fail(400, "You cannot invite yourself.");

        // 6. Check existing membership
        var existing = await memberRepository.GetMemberByUserAndEventAsync(request.UserId, eventId);
        if (existing is not null)
        {
            if (existing.Status is EventMemberStatus.Active or EventMemberStatus.Invited)
                return ApiResponse<TeamMemberDto>.Fail(409,
                    "This user already has an active or pending invitation for this event.");

            // Re-invite: delete old Declined/Removed record
            memberRepository.Remove(existing);
        }

        // 7. Quota check
        var memberCount = await memberRepository.CountActiveAndInvitedByEventAsync(eventId);
        if (memberCount >= MaxTeamMembers)
            return ApiResponse<TeamMemberDto>.Fail(422,
                $"Team member quota exceeded. Maximum {MaxTeamMembers} active or invited members allowed.");

        // 8. Create invitation
        EventTeamMember member;
        try
        {
            member = EventTeamMember.CreateInvitation(
                eventId: eventId,
                userId: request.UserId,
                role: request.Role,
                invitedByUserId: currentUserId,
                customTitle: request.CustomTitle,
                customPermissions: request.Permissions,
                userFullName: request.UserFullName,
                userEmail: request.UserEmail);
        }
        catch (DomainException ex)
        {
            return ApiResponse<TeamMemberDto>.Fail(400, ex.Message);
        }

        memberRepository.AddMember(member);
        await memberRepository.SaveChangesAsync();

        // 9. Publish event for notification service (best-effort)
        await publishEndpoint.Publish(new TeamMemberInvitedEvent(
            EventId: eventId,
            EventTitle: ev.Title ?? string.Empty,
            InvitedUserId: request.UserId,
            InvitedUserEmail: request.UserEmail ?? string.Empty,
            InvitedUserName: request.UserFullName ?? string.Empty,
            Role: request.Role.ToString(),
            CustomTitle: request.CustomTitle,
            InviteToken: member.InviteToken!,
            InviteTokenExpiry: member.InviteTokenExpiry!.Value,
            InvitedByUserId: currentUserId,
            InvitedByUserName: currentUserName ?? string.Empty));

        return ApiResponse<TeamMemberDto>.Success(201, "Invitation sent successfully.", ToDto(member));
    }

    public async Task<ApiResponse<List<TeamMemberDto>>> GetByEventIdAsync(Guid eventId, Guid currentUserId)
    {
        var ev = await eventRepository.GetEventByIdAsync(eventId);
        if (ev is null)
            return ApiResponse<List<TeamMemberDto>>.Fail(404, "Event not found.");

        bool isOrganizer = ev.OrganizerId == currentUserId;
        if (!isOrganizer)
        {
            var currentMember = await memberRepository.GetMemberByUserAndEventAsync(currentUserId, eventId);
            if (currentMember is null || currentMember.Status != EventMemberStatus.Active)
                return ApiResponse<List<TeamMemberDto>>.Fail(403, "You are not a member of this event's team.");
        }

        var members = await memberRepository.GetMembersByEventIdAsync(eventId);
        var dtos = members.Select(ToDto).ToList();
        return ApiResponse<List<TeamMemberDto>>.Success(200, "Success.", dtos);
    }

    public async Task<ApiResponse<string>> AcceptByTokenAsync(string token)
    {
        var member = await memberRepository.GetByInviteTokenAsync(token);
        if (member is null)
            return ApiResponse<string>.Fail(404, "Invitation not found.");

        try
        {
            member.AcceptInvitation();
        }
        catch (DomainException ex)
        {
            return ApiResponse<string>.Fail(400, ex.Message);
        }

        await memberRepository.SaveChangesAsync();
        return ApiResponse<string>.Success(200, "Invitation accepted successfully.", "Accepted");
    }

    public async Task<ApiResponse<string>> DeclineByTokenAsync(string token)
    {
        var member = await memberRepository.GetByInviteTokenAsync(token);
        if (member is null)
            return ApiResponse<string>.Fail(404, "Invitation not found.");

        try
        {
            member.DeclineInvitation();
        }
        catch (DomainException ex)
        {
            return ApiResponse<string>.Fail(400, ex.Message);
        }

        await memberRepository.SaveChangesAsync();
        return ApiResponse<string>.Success(200, "Invitation declined.", "Declined");
    }

    public async Task<ApiResponse<string>> RespondByUserAsync(Guid eventId, Guid currentUserId, string action)
    {
        var member = await memberRepository.GetMemberByUserAndEventAsync(currentUserId, eventId);
        if (member is null || member.Status != EventMemberStatus.Invited)
            return ApiResponse<string>.Fail(404, "No pending invitation found for your account on this event.");

        try
        {
            if (action.Equals("accept", StringComparison.OrdinalIgnoreCase))
                member.AcceptInvitation();
            else if (action.Equals("decline", StringComparison.OrdinalIgnoreCase))
                member.DeclineInvitation();
            else
                return ApiResponse<string>.Fail(400, "Invalid action. Use 'accept' or 'decline'.");
        }
        catch (DomainException ex)
        {
            return ApiResponse<string>.Fail(400, ex.Message);
        }

        await memberRepository.SaveChangesAsync();
        return ApiResponse<string>.Success(200, $"Invitation {action}d successfully.", action);
    }

    public async Task<ApiResponse<string>> RemoveMemberAsync(Guid eventId, Guid memberId, Guid currentUserId)
    {
        var ev = await eventRepository.GetEventByIdAsync(eventId);
        if (ev is null)
            return ApiResponse<string>.Fail(404, "Event not found.");

        var target = await memberRepository.GetMemberByIdAsync(memberId);
        if (target is null || target.EventId != eventId)
            return ApiResponse<string>.Fail(404, "Team member not found.");

        bool isOrganizer = ev.OrganizerId == currentUserId;
        if (!isOrganizer)
        {
            var currentMember = await memberRepository.GetMemberByUserAndEventAsync(currentUserId, eventId);
            bool canManageTeam = currentMember is { Status: EventMemberStatus.Active } &&
                                 currentMember.Permissions.TryGetValue(EventPermissions.CanManageTeam, out var v) && v;

            if (!canManageTeam)
                return ApiResponse<string>.Fail(403, "You do not have permission to remove team members.");

            // CoOrganizer cannot remove other CoOrganizers
            if (target.Role == EventRole.CoOrganizer)
                return ApiResponse<string>.Fail(403, "Only the event organizer can remove a CoOrganizer.");
        }

        // Organizer cannot remove themselves
        if (target.UserId == currentUserId && isOrganizer)
            return ApiResponse<string>.Fail(400, "The event organizer cannot remove themselves.");

        target.Status = EventMemberStatus.Removed;
        await memberRepository.SaveChangesAsync();
        return ApiResponse<string>.Success(200, "Team member removed.", "Removed");
    }

    public async Task<ApiResponse<TeamMemberDto>> UpdatePermissionsAsync(
        Guid eventId,
        Guid memberId,
        Guid currentUserId,
        Dictionary<string, bool> permissions)
    {
        var ev = await eventRepository.GetEventByIdAsync(eventId);
        if (ev is null)
            return ApiResponse<TeamMemberDto>.Fail(404, "Event not found.");

        var target = await memberRepository.GetMemberByIdAsync(memberId);
        if (target is null || target.EventId != eventId)
            return ApiResponse<TeamMemberDto>.Fail(404, "Team member not found.");

        bool isOrganizer = ev.OrganizerId == currentUserId;
        if (!isOrganizer)
        {
            var currentMember = await memberRepository.GetMemberByUserAndEventAsync(currentUserId, eventId);
            bool canManageTeam = currentMember is { Status: EventMemberStatus.Active } &&
                                 currentMember.Permissions.TryGetValue(EventPermissions.CanManageTeam, out var v) && v;

            if (!canManageTeam)
                return ApiResponse<TeamMemberDto>.Fail(403, "You do not have permission to update permissions.");

            // CoOrganizer cannot grant organizer-only permissions
            var forbidden = permissions
                .Where(p => EventPermissions.OrganizerOnlyPermissions.Contains(p.Key) && p.Value)
                .Select(p => p.Key)
                .ToList();

            if (forbidden.Count > 0)
                return ApiResponse<TeamMemberDto>.Fail(403,
                    $"You cannot grant the following permissions: {string.Join(", ", forbidden)}.");
        }

        // Merge permissions (only keys that exist in current permissions dict)
        foreach (var (key, value) in permissions)
        {
            if (target.Permissions.ContainsKey(key))
                target.Permissions[key] = value;
        }

        await memberRepository.SaveChangesAsync();
        return ApiResponse<TeamMemberDto>.Success(200, "Permissions updated.", ToDto(target));
    }

    private static TeamMemberDto ToDto(EventTeamMember m) => new(
        Id: m.Id,
        UserId: m.UserId,
        EventId: m.EventId,
        Role: m.Role.ToString(),
        CustomTitle: m.CustomTitle,
        Permissions: m.Permissions,
        Status: m.Status.ToString(),
        InvitedAt: m.InvitedAt,
        JoinedAt: m.JoinedAt,
        DeclinedAt: m.DeclinedAt,
        InvitedByUserId: m.InvitedByUserId,
        UserFullName: m.UserFullName,
        UserEmail: m.UserEmail
    );
}

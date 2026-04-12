using EventService_Domain.Constants;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class EventTeamMemberRepository(EventServiceDbContext dbContext) : IEventTeamMemberRepository
{
    public async Task<IReadOnlyList<EventTeamMember>> GetMembersByEventIdAsync(Guid eventId)
    {
        return await dbContext.EventTeamMembers
            .AsNoTracking()
            .Where(m => m.EventId == eventId)
            .ToListAsync();
    }

    public async Task<EventTeamMember?> GetMemberByIdAsync(Guid memberId)
    {
        return await dbContext.EventTeamMembers
            .FirstOrDefaultAsync(m => m.Id == memberId);
    }

    public async Task<Dictionary<string,bool>?> GetPermissionsByMemberIdAsync(Guid eventId, Guid memberId)
    {
        return await dbContext.EventTeamMembers.Where(m =>
                m.EventId == eventId && m.UserId == memberId && m.Status == EventMemberStatus.Active)
            .Select(m => m.Permissions)
            .FirstOrDefaultAsync();
    }

    public IQueryable<EventTeamMember> GetQueryable()
    {
        return dbContext.EventTeamMembers;
    }

    public async Task<EventTeamMember?> GetMemberByUserAndEventAsync(Guid userId, Guid eventId)
    {
        return await dbContext.EventTeamMembers
            .FirstOrDefaultAsync(m => m.UserId == userId && m.EventId == eventId);
    }

    public EventTeamMember AddMember(EventTeamMember member)
    {
        return dbContext.EventTeamMembers.Add(member).Entity;
    }

    public EventTeamMember UpdateMember(EventTeamMember member)
    {
        return dbContext.EventTeamMembers.Update(member).Entity;
    }

    public async Task<bool> DeleteMemberAsync(Guid memberId)
    {
        var member = await dbContext.EventTeamMembers.FindAsync(memberId);
        if (member is null) return false;
        dbContext.EventTeamMembers.Remove(member);
        return true;
    }

    public async Task<bool> MemberExistsAsync(Guid userId, Guid eventId)
    {
        return await dbContext.EventTeamMembers
            .AnyAsync(m => m.UserId == userId && m.EventId == eventId);
    }

    public IQueryable<EventTeamMember> GetQueryableByUserId(Guid userId)
    {
        return dbContext.EventTeamMembers.Where(m => m.UserId == userId).AsNoTracking();
    }

    public async Task<EventTeamMember?> GetByInviteTokenAsync(string token)
    {
        return await dbContext.EventTeamMembers
            .FirstOrDefaultAsync(m => m.InviteToken == token);
    }

    public async Task<int> CountActiveAndInvitedByEventAsync(Guid eventId)
    {
        return await dbContext.EventTeamMembers
            .AsNoTracking()
            .CountAsync(m => m.EventId == eventId &&
                             (m.Status == EventMemberStatus.Active || m.Status == EventMemberStatus.Invited));
    }

    public void Remove(EventTeamMember member)
    {
        dbContext.EventTeamMembers.Remove(member);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}

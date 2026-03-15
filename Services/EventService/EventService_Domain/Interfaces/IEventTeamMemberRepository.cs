using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface IEventTeamMemberRepository
{
    Task<IReadOnlyList<EventTeamMember>> GetMembersByEventIdAsync(Guid eventId);
    Task<EventTeamMember?> GetMemberByIdAsync(Guid memberId);
    Task<EventTeamMember?> GetMemberByUserAndEventAsync(Guid userId, Guid eventId);
    EventTeamMember AddMember(EventTeamMember member);
    EventTeamMember UpdateMember(EventTeamMember member);
    Task<bool> DeleteMemberAsync(Guid memberId);
    Task<bool> MemberExistsAsync(Guid userId, Guid eventId);
    IQueryable<EventTeamMember> GetQueryableByUserId(Guid userId);
    Task SaveChangesAsync();
}

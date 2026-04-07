using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface ILineupRepository
    {
        Task<Lineup> AddLineupAsync(Lineup lineup);
        Task<Lineup?> GetLineupByIdAsync(Guid lineupId);
        Task<List<Lineup>> GetAllLineupsAsync();
        Task<List<Lineup>> GetLineupsByEventIdAsync(Guid eventId);
        Task<Lineup> UpdateLineupAsync(Lineup lineup);
        Task<bool> DeleteLineupAsync(Guid lineupId);
        Task<List<Lineup>> GetLineupsByEventAndTalentsAsync(Guid eventId, Guid? sessionId, List<Guid> talentIds);
        Task<bool> LineupExistsAsync(Guid eventId, Guid? sessionId, Guid talentId);
        Task<List<Lineup>> GetLineupsByEventIdWithDetailsAsync(Guid eventId);
    }
}

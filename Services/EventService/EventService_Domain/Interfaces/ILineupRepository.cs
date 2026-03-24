using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface ILineupRepository
    {
        Task<Lineup> AddLineupAsync(Lineup lineup);
        Task<Lineup?> GetLineupByIdAsync(Guid lineupId);
        Task<PagedResult<Lineup>> GetLineupsByEventIdAsync(Guid eventId, int pageNumber, int pageSize, string? sortBy = null);
        Task<PagedResult<Lineup>> GetAllLineupsAsync(int pageNumber, int pageSize, string? sortBy = null);
        Task<Lineup> UpdateLineupAsync(Lineup lineup);
        Task<bool> DeleteLineupAsync(Guid lineupId);
        Task<List<Lineup>> GetLineupsByEventAndTalentsAsync(Guid eventId, Guid? sessionId, List<Guid> talentIds);
        Task<bool> LineupExistsAsync(Guid eventId, Guid? sessionId, Guid talentId);
    }
}

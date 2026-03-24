using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface IPollRepository
    {
        Task<Poll> AddPollAsync(Poll poll);
        Task<bool> DeletePollAsync(Poll poll);
        Task<Poll?> GetPollByIdAsync(Guid pollId);
        Task<PagedResult<Poll>> GetPollsBySessionIdAsync(Guid sessionId, int pageNumber, int pageSize, string? sortBy = null)
        Task<Poll> UpdatePollAsync(Poll poll);
    }
}

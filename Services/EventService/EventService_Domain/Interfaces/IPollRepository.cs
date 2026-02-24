using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface IPollRepository
    {
        Task<Poll> AddPollAsync(Poll poll);
        Task<bool> DeletePollAsync(Poll poll);
        Task<Poll?> GetPollByIdAsync(Guid pollId);
        Task<IEnumerable<Poll>> GetPollsBySessionIdAsync(Guid sessionId);
        Task<Poll> UpdatePollAsync(Poll poll);
    }
}

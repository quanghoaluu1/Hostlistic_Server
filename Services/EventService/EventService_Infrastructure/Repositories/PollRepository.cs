using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories
{
    public class PollRepository : IPollRepository
    {
        private readonly EventServiceDbContext _context;
        public PollRepository(EventServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Poll> AddPollAsync(Poll poll)
        {
            await _context.Polls.AddAsync(poll);
            await _context.SaveChangesAsync();
            return poll;
        }

        public async Task<bool> DeletePollAsync(Poll poll)
        {
            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Poll?> GetPollByIdAsync(Guid pollId)
        {
            return await _context.Polls.FindAsync(pollId);
        }

        public async Task<IEnumerable<Poll>> GetPollsBySessionIdAsync(Guid sessionId)
        {
            return await _context.Polls
                .Where(p => p.SessionId == sessionId).ToListAsync();
        }

        public async Task<Poll> UpdatePollAsync(Poll poll)
        {
            _context.Polls.Update(poll);
            await _context.SaveChangesAsync();
            return poll;
        }
    }
}

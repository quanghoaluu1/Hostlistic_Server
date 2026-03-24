using Common;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories
{
    public class LineupRepository : ILineupRepository
    {
        private readonly EventServiceDbContext _context;
        public LineupRepository(EventServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Lineup> AddLineupAsync(Lineup lineup)
        {
            await _context.Lineups.AddAsync(lineup);
            await _context.SaveChangesAsync();
            return lineup;
        }

        public async Task<Lineup?> GetLineupByIdAsync(Guid lineupId)
        {
            return await _context.Lineups.Include(l => l.Talent).FirstOrDefaultAsync(t => t.Id == lineupId);
        }

        public async Task<PagedResult<Lineup>> GetAllLineupsAsync(int pageNumber, int pageSize, string? sortBy = null)
        {
            var query = _context.Lineups.Include(l => l.Talent).AsQueryable();
            query = query.ApplySorting(sortBy);
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<PagedResult<Lineup>> GetLineupsByEventIdAsync(Guid eventId, int pageNumber, int pageSize, string? sortBy = null)
        {
            var query = _context.Lineups.Include(l => l.Talent)
                .Where(l => l.EventId == eventId)
                .AsQueryable();
            query = query.ApplySorting(sortBy);
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Lineup> UpdateLineupAsync(Lineup lineup)
        {
            _context.Lineups.Update(lineup);
            await _context.SaveChangesAsync();
            return lineup;
        }

        public async Task<bool> DeleteLineupAsync(Guid lineupId)
        {
            var lineup = await _context.Lineups.FirstOrDefaultAsync(t => t.Id == lineupId);
            if (lineup == null)
                return false;
            _context.Lineups.Remove(lineup);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Lineup>> GetLineupsByEventAndTalentsAsync(Guid eventId, Guid? sessionId, List<Guid> talentIds)
        {
            var query = _context.Lineups.AsQueryable();
            query = query.Where(l => l.EventId == eventId && talentIds.Contains(l.TalentId));
            if (sessionId.HasValue)
            {
                query = query.Where(l => l.SessionId == sessionId.Value);
            }
            else
            {
                query = query.Where(l => l.SessionId == null);
            }
            return await query.ToListAsync();
        }

        public async Task<bool> LineupExistsAsync(Guid eventId, Guid? sessionId, Guid talentId)
        {
            return await _context.Lineups.AnyAsync(l =>
                l.EventId == eventId &&
                l.SessionId == sessionId &&
                l.TalentId == talentId
            );
        }
    }
}

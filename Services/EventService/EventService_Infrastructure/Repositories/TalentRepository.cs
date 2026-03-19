using Common;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories
{
    public class TalentRepository : ITalentRepository
    {
        private readonly EventServiceDbContext _context;
        public TalentRepository(EventServiceDbContext context)
        {
            _context = context;
        }
        public async Task<Talent> AddTalentAsync(Talent talent)
        {
            await _context.Talents.AddAsync(talent);
            return talent;
        }

        public async Task<bool> DeleteTalentAsync(Guid talentId)
        {
            var talent = await _context.Talents.FindAsync(talentId);
            if (talent == null)
                return false;
            _context.Talents.Remove(talent);
            return true;
        }

        public async Task<IEnumerable<Talent>> GetAllTalentsAsync()
        {
            return await _context.Talents
                .Include(t => t.Lineups)
                .ToListAsync();
        }

        public async Task<PagedResult<Talent>> GetAllTalentsAsync(string? search, int pageNumber, int pageSize, string? sortBy = null)
        {
            var query = _context.Talents
                .Include(t => t.Lineups)
                .AsQueryable();

            // FILTER
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Name.Contains(search));
            }

            // SORT (reuse extension)
            query = query.ApplySorting(sortBy);

            // PAGING (reuse extension)
            return await query.ToPagedResultAsync(pageNumber, pageSize);
        }

        public async Task<Talent> GetTalentByIdAsync(Guid talentId)
        {
            return await _context.Talents
                .Include(t => t.Lineups)
                .FirstOrDefaultAsync(t => t.Id == talentId);
        }

        public async Task<List<Talent>> GetTalentByIdAsync(List<Guid> talentIds)
        {
            return await _context.Talents
                .Where(t => talentIds.Contains(t.Id))
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TalentExistsAsync(Guid talentId)
        {
            return await _context.Talents.AnyAsync(t => t.Id == talentId);
        }

        public async Task<Talent> UpdateTalentAsync(Talent talent)
        {
            _context.Talents.Update(talent);
            return talent;

        }
    }
}

using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories
{
    public class VenueRepository : IVenueRepository
    {
        private readonly EventServiceDbContext _context;
        public VenueRepository(EventServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Venue?> GetByIdWithinEventAsync(Guid eventId, Guid venueId)
        {
            return await _context.Venues
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == venueId && v.EventId == eventId);
        }
        public async Task<Venue?> GetByIdWithinEventForUpdateAsync(Guid eventId, Guid venueId)
        {
            return await _context.Venues
                .FirstOrDefaultAsync(v => v.Id == venueId && v.EventId == eventId);
            // NO AsNoTracking → EF tracks changes
        }
        public async Task<IReadOnlyList<Venue>> GetByEventIdAsync(Guid eventId)
        {
            return await _context.Venues
                .AsNoTracking()
                .Where(v => v.EventId == eventId)
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(Guid eventId, string name, Guid? excludeVenueId = null)
        {
            return await _context.Venues
                .AnyAsync(v => v.EventId == eventId
                               && v.Name == name
                               && (excludeVenueId == null || v.Id != excludeVenueId));
        }

        public async Task<Venue> AddVenueAsync(Venue venue)
        {
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return venue;
        }

        public async Task<Venue> GetVenueByIdAsync(Guid id)
        {
            return await _context.Venues.FindAsync(id);
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
        {
            return await _context.Venues.ToListAsync();
        }

        public async Task<Venue> UpdateVenueAsync(Venue venue)
        {
            _context.Venues.Update(venue);
            await _context.SaveChangesAsync();
            return venue;
        }

        public async Task<bool> DeleteVenueAsync(Guid id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}

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

using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class CheckInRepository : ICheckInRepository
{
    private readonly EventServiceDbContext _context;

    public CheckInRepository(EventServiceDbContext context)
    {
        _context = context;
    }

    public async Task<CheckIn?> GetCheckInByIdAsync(Guid checkInId)
    {
        return await _context.CheckIns
            .Include(c => c.Event)
            .Include(c => c.Session)
            .FirstOrDefaultAsync(c => c.Id == checkInId);
    }

    public async Task<IEnumerable<CheckIn>> GetCheckInsByEventIdAsync(Guid eventId)
    {
        return await _context.CheckIns
            .Include(c => c.Session)
            .Where(c => c.EventId == eventId)
            .OrderByDescending(c => c.CheckedInAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CheckIn>> GetCheckInsBySessionIdAsync(Guid sessionId)
    {
        return await _context.CheckIns
            .Include(c => c.Event)
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.CheckedInAt)
            .ToListAsync();
    }

    public async Task<CheckIn?> GetCheckInByTicketIdAsync(Guid ticketId)
    {
        return await _context.CheckIns
            .FirstOrDefaultAsync(c => c.TicketId == ticketId);
    }

    public async Task<CheckIn> AddCheckInAsync(CheckIn checkIn)
    {
        checkIn.Id = Guid.NewGuid();
        await _context.CheckIns.AddAsync(checkIn);
        return checkIn;
    }

    public async Task<CheckIn> UpdateCheckInAsync(CheckIn checkIn)
    {
        _context.CheckIns.Update(checkIn);
        return checkIn;
    }

    public async Task<bool> DeleteCheckInAsync(Guid checkInId)
    {
        var checkIn = await _context.CheckIns.FindAsync(checkInId);
        if (checkIn == null)
            return false;

        _context.CheckIns.Remove(checkIn);
        return true;
    }

    public async Task<bool> CheckInExistsAsync(Guid checkInId)
    {
        return await _context.CheckIns.AnyAsync(c => c.Id == checkInId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

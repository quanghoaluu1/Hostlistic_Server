using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class EventSettlementRepository : IEventSettlementRepository
{
    private readonly BookingServiceDbContext _context;

    public EventSettlementRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<EventSettlement?> GetByIdAsync(Guid id)
    {
        return await _context.EventSettlements.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<EventSettlement?> GetByEventIdAsync(Guid eventId)
    {
        return await _context.EventSettlements.FirstOrDefaultAsync(e => e.EventId == eventId);
    }

    public async Task<IEnumerable<EventSettlement>> GetByOrganizerIdAsync(Guid organizerId)
    {
        return await _context.EventSettlements
            .Where(e => e.OrganizerId == organizerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<EventSettlement>> GetByStatusAsync(SettlementStatus status)
    {
        return await _context.EventSettlements
            .Where(e => e.Status == status)
            .ToListAsync();
    }

    public async Task<EventSettlement> AddAsync(EventSettlement settlement)
    {
        settlement.Id = Guid.NewGuid();
        settlement.CreatedAt = DateTime.UtcNow;
        await _context.EventSettlements.AddAsync(settlement);
        return settlement;
    }

    public Task<EventSettlement> UpdateAsync(EventSettlement settlement)
    {
        _context.EventSettlements.Update(settlement);
        return Task.FromResult(settlement);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

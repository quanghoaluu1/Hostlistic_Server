using Common;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class TicketTypeRepository : ITicketTypeRepository
{
    private readonly EventServiceDbContext _context;

    public TicketTypeRepository(EventServiceDbContext context)
    {
        _context = context;
    }

    public async Task<TicketType?> GetTicketTypeByIdAsync(Guid ticketTypeId)
    {
        return await _context.TicketTypes
            .Include(t => t.Event)
            .Include(t => t.Session)
            .FirstOrDefaultAsync(t => t.Id == ticketTypeId);
    }

    public async Task<PagedResult<TicketType>> GetTicketTypesByEventIdAsync(Guid eventId, BaseQueryParams request)
    {
        var query = _context.TicketTypes
            .Include(t => t.Session)
            .Where(t => t.EventId == eventId)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<PagedResult<TicketType>> GetTicketTypesBySessionIdAsync(Guid sessionId, BaseQueryParams request)
    {
        var query = _context.TicketTypes
            .Include(t => t.Event)
            .Where(t => t.SessionId == sessionId)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<TicketType> AddTicketTypeAsync(TicketType ticketType)
    {
        ticketType.Id = Guid.NewGuid();
        await _context.TicketTypes.AddAsync(ticketType);
        return ticketType;
    }

    public async Task<TicketType> UpdateTicketTypeAsync(TicketType ticketType)
    {
        _context.TicketTypes.Update(ticketType);
        return ticketType;
    }

    public async Task<bool> DeleteTicketTypeAsync(Guid ticketTypeId)
    {
        var ticketType = await _context.TicketTypes.FindAsync(ticketTypeId);
        if (ticketType == null)
            return false;

        _context.TicketTypes.Remove(ticketType);
        return true;
    }

    public async Task<bool> TicketTypeExistsAsync(Guid ticketTypeId)
    {
        return await _context.TicketTypes.AnyAsync(t => t.Id == ticketTypeId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

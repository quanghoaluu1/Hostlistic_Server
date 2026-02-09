using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly BookingServiceDbContext _context;

    public TicketRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetTicketByIdAsync(Guid ticketId)
    {
        return await _context.Tickets
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    public async Task<Ticket?> GetTicketByCodeAsync(string ticketCode)
    {
        return await _context.Tickets
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByOrderIdAsync(Guid orderId)
    {
        return await _context.Tickets
            .Include(t => t.Order)
            .Where(t => t.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<Ticket> AddTicketAsync(Ticket ticket)
    {
        ticket.Id = Guid.NewGuid();
        ticket.IssuedDate = DateTime.UtcNow;
        ticket.TicketCode = GenerateTicketCode();
        await _context.Tickets.AddAsync(ticket);
        return ticket;
    }

    public Task<Ticket> UpdateTicketAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        return Task.FromResult(ticket);
    }

    public async Task<bool> DeleteTicketAsync(Guid ticketId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            return false;

        _context.Tickets.Remove(ticket);
        return true;
    }

    public async Task<bool> TicketExistsAsync(Guid ticketId)
    {
        return await _context.Tickets.AnyAsync(t => t.Id == ticketId);
    }

    public async Task<bool> TicketCodeExistsAsync(string ticketCode)
    {
        return await _context.Tickets.AnyAsync(t => t.TicketCode == ticketCode);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    private string GenerateTicketCode()
    {
        return $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
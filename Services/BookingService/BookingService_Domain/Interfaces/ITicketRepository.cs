using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetTicketByIdAsync(Guid ticketId);
    Task<Ticket?> GetTicketByCodeAsync(string ticketCode);
    Task<IEnumerable<Ticket>> GetTicketsByOrderIdAsync(Guid orderId);
    Task<Ticket> AddTicketAsync(Ticket ticket);
    Task<Ticket> UpdateTicketAsync(Ticket ticket);
    Task<bool> DeleteTicketAsync(Guid ticketId);
    Task<bool> TicketExistsAsync(Guid ticketId);
    Task<bool> TicketCodeExistsAsync(string ticketCode);
    Task SaveChangesAsync();
}
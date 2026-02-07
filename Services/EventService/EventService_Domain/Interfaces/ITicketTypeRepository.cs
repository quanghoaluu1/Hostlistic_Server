using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ITicketTypeRepository
{
    Task<TicketType?> GetTicketTypeByIdAsync(Guid ticketTypeId);
    Task<IEnumerable<TicketType>> GetTicketTypesByEventIdAsync(Guid eventId);
    Task<IEnumerable<TicketType>> GetTicketTypesBySessionIdAsync(Guid sessionId);
    Task<TicketType> AddTicketTypeAsync(TicketType ticketType);
    Task<TicketType> UpdateTicketTypeAsync(TicketType ticketType);
    Task<bool> DeleteTicketTypeAsync(Guid ticketTypeId);
    Task<bool> TicketTypeExistsAsync(Guid ticketTypeId);
    Task SaveChangesAsync();
}

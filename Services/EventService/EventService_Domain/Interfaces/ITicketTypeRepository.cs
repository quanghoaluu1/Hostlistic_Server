using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ITicketTypeRepository
{
    Task<TicketType?> GetTicketTypeByIdAsync(Guid ticketTypeId);
    Task<PagedResult<TicketType>> GetTicketTypesByEventIdAsync(Guid eventId, BaseQueryParams request);
    Task<PagedResult<TicketType>> GetTicketTypesBySessionIdAsync(Guid sessionId, BaseQueryParams request);
    Task<TicketType> AddTicketTypeAsync(TicketType ticketType);
    Task<TicketType> UpdateTicketTypeAsync(TicketType ticketType);
    Task<bool> DeleteTicketTypeAsync(Guid ticketTypeId);
    Task<bool> TicketTypeExistsAsync(Guid ticketTypeId);
    Task SaveChangesAsync();
}

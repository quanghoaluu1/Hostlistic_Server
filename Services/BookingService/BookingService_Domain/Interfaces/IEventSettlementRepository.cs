using BookingService_Domain.Entities;
using BookingService_Domain.Enum;

namespace BookingService_Domain.Interfaces;

public interface IEventSettlementRepository
{
    Task<EventSettlement?> GetByIdAsync(Guid id);
    Task<EventSettlement?> GetByEventIdAsync(Guid eventId);
    Task<IEnumerable<EventSettlement>> GetAllAsync();
    Task<IEnumerable<Guid>> GetEventIds();
    Task<IEnumerable<EventSettlement>> GetByOrganizerIdAsync(Guid organizerId);
    Task<EventSettlement?> GetByEventIdAndStatusAsync(Guid eventId, SettlementStatus status);
    Task<IEnumerable<EventSettlement>> GetByStatusAsync(SettlementStatus status);
    Task<EventSettlement> AddAsync(EventSettlement settlement);
    Task<EventSettlement> UpdateAsync(EventSettlement settlement);
    Task SaveChangesAsync();
}

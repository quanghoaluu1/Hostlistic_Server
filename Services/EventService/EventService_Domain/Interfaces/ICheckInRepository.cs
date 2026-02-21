using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ICheckInRepository
{
    Task<CheckIn?> GetCheckInByIdAsync(Guid checkInId);
    Task<IEnumerable<CheckIn>> GetCheckInsByEventIdAsync(Guid eventId);
    Task<IEnumerable<CheckIn>> GetCheckInsBySessionIdAsync(Guid sessionId);
    Task<CheckIn?> GetCheckInByTicketIdAsync(Guid ticketId);
    Task<CheckIn> AddCheckInAsync(CheckIn checkIn);
    Task<CheckIn> UpdateCheckInAsync(CheckIn checkIn);
    Task<bool> DeleteCheckInAsync(Guid checkInId);
    Task<bool> CheckInExistsAsync(Guid checkInId);
    Task SaveChangesAsync();
}

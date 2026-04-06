using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class SessionBookingRepository : ISessionBookingRepository
{
    private readonly EventServiceDbContext _context;

    public SessionBookingRepository(EventServiceDbContext context)
    {
        _context = context;
    }

    public async Task<SessionBooking?> GetSessionBookingByIdAsync(Guid bookingId)
    {
        return await _context.SessionBookings
            .AsNoTracking()
            .Include(sb => sb.Session)
            .ThenInclude(s => s.Track)
            .Include(sb => sb.Session)
            .ThenInclude(s => s.Venue)
            .FirstOrDefaultAsync(sb => sb.Id == bookingId);
    }

    //Lấy danh sách các lượt đặt chỗ theo Session
    public async Task<IEnumerable<SessionBooking>> GetSessionBookingsBySessionIdAsync(Guid sessionId)
    {
        return await _context.SessionBookings
            .AsNoTracking()
            .Include(sb => sb.Session)
            .Where(sb => sb.SessionId == sessionId)
            .ToListAsync();
    }

    //Lấy danh sách các lượt đặt chỗ của một người dùng
    public async Task<IEnumerable<SessionBooking>> GetSessionBookingsByUserIdAsync(Guid userId)
    {
        return await _context.SessionBookings
            .AsNoTracking()
            .Include(sb => sb.Session)
            .Where(sb => sb.UserId == userId)
            .ToListAsync();
    }

    public async Task<SessionBooking?> GetByUserAndSessionAsync(Guid userId, Guid sessionId)
    {
        return await _context.SessionBookings
            .AsNoTracking()
            .Include(sb => sb.Session)
            .FirstOrDefaultAsync(sb => sb.UserId == userId && sb.SessionId == sessionId);
    }

    public async Task<List<SessionBooking>> GetByUserAndEventAsync(Guid userId, Guid eventId)
    {
        return await _context.SessionBookings
            .AsNoTracking()
            .Include(sb => sb.Session)
            .ThenInclude(s => s.Track)
            .Include(sb => sb.Session)
            .ThenInclude(s => s.Venue)
            .Where(sb => sb.UserId == userId
                         && sb.Session.EventId == eventId
                         && sb.Status == BookingStatus.Confirmed)
            .OrderBy(sb => sb.Session.StartTime)
            .ToListAsync();
    }

    public async Task<List<Session>> GetConflictingSessionsAsync(Guid userId, Guid eventId, DateTime start, DateTime end,
        Guid? excludeSessionId = null)
    {
        var query = _context.SessionBookings
            .AsNoTracking()
            .Where(sb => sb.UserId == userId
                         && sb.Status == BookingStatus.Confirmed
                         && sb.Session.EventId == eventId
                         && sb.Session.StartTime != null && sb.Session.EndTime != null
                         && sb.Session.StartTime < end
                         && sb.Session.EndTime > start);

        if (excludeSessionId.HasValue)
            query = query.Where(sb => sb.SessionId != excludeSessionId.Value);

        return await query
            .Select(sb => sb.Session)
            .ToListAsync();
    }

    public async Task<HashSet<Guid>> GetBookedSessionIdsAsync(Guid userId, Guid eventId)
    {
        var ids = await _context.SessionBookings
            .AsNoTracking()
            .Where(sb => sb.UserId == userId
                         && sb.Session.EventId == eventId
                         && sb.Status == BookingStatus.Confirmed)
            .Select(sb => sb.SessionId)
            .ToListAsync();

        return [.. ids];
    }

    //Tạo mới một lượt đặt chỗ cho session
    public async Task<SessionBooking> AddSessionBookingAsync(SessionBooking sessionBooking)
    {
        sessionBooking.BookingDate = DateTime.UtcNow;
        await _context.SessionBookings.AddAsync(sessionBooking);
        return sessionBooking;
    }

    public async Task<SessionBooking> UpdateSessionBookingAsync(SessionBooking sessionBooking)
    {
        _context.SessionBookings.Update(sessionBooking);
        return sessionBooking;
    }

    public async Task<bool> DeleteSessionBookingAsync(Guid bookingId)
    {
        var sessionBooking = await _context.SessionBookings.FindAsync(bookingId);
        if (sessionBooking == null)
            return false;

        _context.SessionBookings.Remove(sessionBooking);
        return true;
    }

    public async Task<bool> SessionBookingExistsAsync(Guid bookingId)
    {
        return await _context.SessionBookings.AnyAsync(sb => sb.Id == bookingId);
    }

    //Kiểm tra user đã đặt chỗ cho session hay chưa
    public async Task<bool> UserHasBookingForSessionAsync(Guid userId, Guid sessionId)
    {
        return await _context.SessionBookings
            .AnyAsync(sb => sb.UserId == userId
                            && sb.SessionId == sessionId
                            && sb.Status == BookingStatus.Confirmed);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

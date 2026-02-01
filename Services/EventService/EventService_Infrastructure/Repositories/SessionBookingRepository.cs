using EventService_Domain.Entities;
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
            .Include(sb => sb.Session)
            .FirstOrDefaultAsync(sb => sb.Id == bookingId);
    }

    //Lấy danh sách các lượt đặt chỗ theo Session
    public async Task<IEnumerable<SessionBooking>> GetSessionBookingsBySessionIdAsync(Guid sessionId)
    {
        return await _context.SessionBookings
            .Include(sb => sb.Session)
            .Where(sb => sb.SessionId == sessionId)
            .ToListAsync();
    }

    //Lấy danh sách các lượt đặt chỗ của một người dùng
    public async Task<IEnumerable<SessionBooking>> GetSessionBookingsByUserIdAsync(Guid userId)
    {
        return await _context.SessionBookings
            .Include(sb => sb.Session)
            .Where(sb => sb.UserId == userId)
            .ToListAsync();
    }

    //Lấy thông tin đặt chỗ của user cho một session cụ thể
    public async Task<SessionBooking?> GetSessionBookingByUserAndSessionAsync(Guid userId, Guid sessionId)
    {
        return await _context.SessionBookings
            .Include(sb => sb.Session)
            .FirstOrDefaultAsync(sb => sb.UserId == userId && sb.SessionId == sessionId);
    }

    //Tạo mới một lượt đặt chỗ cho session
    public async Task<SessionBooking> AddSessionBookingAsync(SessionBooking sessionBooking)
    {
        sessionBooking.Id = Guid.NewGuid();
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
        return await _context.SessionBookings.AnyAsync(sb => sb.UserId == userId && sb.SessionId == sessionId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
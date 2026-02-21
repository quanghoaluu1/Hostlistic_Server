using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly BookingServiceDbContext _context;

    public OrderRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Tickets)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByEventIdAsync(Guid eventId)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Tickets)
            .Include(o => o.Payments)
            .Where(o => o.EventId == eventId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Tickets)
            .Include(o => o.Payments)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    public async Task<Order> AddOrderAsync(Order order)
    {
        order.Id = Guid.NewGuid();
        order.OrderDate = DateTime.UtcNow;
        await _context.Orders.AddAsync(order);
        return order;
    }

    public Task<Order> UpdateOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        return Task.FromResult(order);
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        return true;
    }

    public async Task<bool> OrderExistsAsync(Guid orderId)
    {
        return await _context.Orders.AnyAsync(o => o.Id == orderId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
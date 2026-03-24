using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class OrderDetailRepository : IOrderDetailRepository
{
    private readonly BookingServiceDbContext _context;

    public OrderDetailRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDetail?> GetOrderDetailByIdAsync(Guid orderDetailId)
    {
        return await _context.OrderDetails
            .Include(od => od.Order)
            .FirstOrDefaultAsync(od => od.Id == orderDetailId);
    }

    public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(Guid orderId)
    {
        return await _context.OrderDetails
            .Include(od => od.Order)
            .Where(od => od.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderDetail>> GetByOrderIds(List<Guid> orderIds)
    {
        return await _context.OrderDetails.Include(od => od.Order)
            .Where(od => orderIds.Contains(od.OrderId))
            .ToListAsync();
    }
}
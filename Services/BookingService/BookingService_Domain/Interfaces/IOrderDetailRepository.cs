using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface IOrderDetailRepository
{
    Task<OrderDetail?> GetOrderDetailByIdAsync(Guid orderDetailId);
    Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(Guid orderId);
}
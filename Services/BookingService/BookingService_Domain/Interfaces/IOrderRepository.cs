using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task<IEnumerable<Order>> GetOrdersByEventIdAsync(Guid eventId);
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
    Task<IEnumerable<Order>> GetConfirmedOrdersByEventIdAsync(Guid eventId);
    Task<Order?> GetOrderByOrderCodeAsync(long orderCode);
    Task<Order> AddOrderAsync(Order order);
    Task<Order> UpdateOrderAsync(Order order);
    Task<bool> DeleteOrderAsync(Guid orderId);
    Task<bool> OrderExistsAsync(Guid orderId);
    Task SaveChangesAsync();
}
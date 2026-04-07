using Common;
using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid orderId);
    Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersByEventIdAsync(Guid eventId);
    Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersByUserIdAsync(Guid userId);
    Task<ApiResponse<OrderDto>> GetOrderByPayOsCodeAsync(long orderCode);
    Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderRequest request);
    Task<ApiResponse<OrderDto>> UpdateOrderAsync(Guid orderId, UpdateOrderRequest request);
    Task<ApiResponse<bool>> DeleteOrderAsync(Guid orderId);
}
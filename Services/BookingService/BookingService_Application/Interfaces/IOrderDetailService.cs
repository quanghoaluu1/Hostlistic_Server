using Common;
using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface IOrderDetailService
{
    Task<ApiResponse<OrderDetailDto>> GetOrderDetailByIdAsync(Guid orderDetailId);
    Task<ApiResponse<IEnumerable<OrderDetailDto>>> GetOrderDetailsByOrderIdAsync(Guid orderId);
}
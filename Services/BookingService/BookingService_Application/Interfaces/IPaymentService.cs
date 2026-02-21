using Common;
using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid paymentId);
    Task<ApiResponse<IEnumerable<PaymentDto>>> GetPaymentsByOrderIdAsync(Guid orderId);
    Task<ApiResponse<PaymentDto>> CreatePaymentAsync(CreatePaymentRequest request);
    Task<ApiResponse<PaymentDto>> UpdatePaymentAsync(Guid paymentId, UpdatePaymentRequest request);
    Task<ApiResponse<bool>> DeletePaymentAsync(Guid paymentId);
}
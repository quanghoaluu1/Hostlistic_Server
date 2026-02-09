using Common;
using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface IPaymentMethodService
{
    Task<ApiResponse<PaymentMethodDto>> GetPaymentMethodByIdAsync(Guid paymentMethodId);
    Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetActivePaymentMethodsAsync();
    Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetAllPaymentMethodsAsync();
    Task<ApiResponse<PaymentMethodDto>> GetPaymentMethodByCodeAsync(string code);
    Task<ApiResponse<PaymentMethodDto>> CreatePaymentMethodAsync(CreatePaymentMethodRequest request);
    Task<ApiResponse<PaymentMethodDto>> UpdatePaymentMethodAsync(Guid paymentMethodId, UpdatePaymentMethodRequest request);
    Task<ApiResponse<bool>> DeletePaymentMethodAsync(Guid paymentMethodId);
}
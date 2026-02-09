using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using Mapster;

namespace BookingService_Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
        if (payment == null)
            return ApiResponse<PaymentDto>.Fail(404, "Payment not found");

        var paymentDto = payment.Adapt<PaymentDto>();
        return ApiResponse<PaymentDto>.Success(200, "Payment retrieved successfully", paymentDto);
    }

    public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetPaymentsByOrderIdAsync(Guid orderId)
    {
        var payments = await _paymentRepository.GetPaymentsByOrderIdAsync(orderId);
        var paymentDtos = payments.Adapt<IEnumerable<PaymentDto>>();
        return ApiResponse<IEnumerable<PaymentDto>>.Success(200, "Payments retrieved successfully", paymentDtos);
    }

    //Add real payment gateway integration later
    public async Task<ApiResponse<PaymentDto>> CreatePaymentAsync(CreatePaymentRequest request)
    {
        // Validate order exists
        var orderExists = await _orderRepository.OrderExistsAsync(request.OrderId);
        if (!orderExists)
            return ApiResponse<PaymentDto>.Fail(404, "Order not found");

        // Validate payment method exists
        var paymentMethodExists = await _paymentMethodRepository.PaymentMethodExistsAsync(request.PaymentMethodId);
        if (!paymentMethodExists)
            return ApiResponse<PaymentDto>.Fail(404, "Payment method not found");

        if (request.Amount <= 0)
            return ApiResponse<PaymentDto>.Fail(400, "Payment amount must be greater than zero");

        var payment = request.Adapt<Payment>();
        payment.Status = BookingService_Domain.Enum.PaymentStatus.Pending;

        await _paymentRepository.AddPaymentAsync(payment);
        await _paymentRepository.SaveChangesAsync();

        var paymentDto = payment.Adapt<PaymentDto>();
        return ApiResponse<PaymentDto>.Success(201, "Payment created successfully", paymentDto);
    }

    public async Task<ApiResponse<PaymentDto>> UpdatePaymentAsync(Guid paymentId, UpdatePaymentRequest request)
    {
        var existingPayment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
        if (existingPayment == null)
            return ApiResponse<PaymentDto>.Fail(404, "Payment not found");

        // Update properties
        existingPayment.Status = request.Status;
        existingPayment.TransactionId = request.TransactionId;

        await _paymentRepository.UpdatePaymentAsync(existingPayment);
        await _paymentRepository.SaveChangesAsync();

        var paymentDto = existingPayment.Adapt<PaymentDto>();
        return ApiResponse<PaymentDto>.Success(200, "Payment updated successfully", paymentDto);
    }

    public async Task<ApiResponse<bool>> DeletePaymentAsync(Guid paymentId)
    {
        var exists = await _paymentRepository.PaymentExistsAsync(paymentId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Payment not found");

        var deleted = await _paymentRepository.DeletePaymentAsync(paymentId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete payment");

        await _paymentRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Payment deleted successfully", true);
    }
}
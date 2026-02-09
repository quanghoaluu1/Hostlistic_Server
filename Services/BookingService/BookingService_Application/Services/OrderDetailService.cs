using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Interfaces;
using Mapster;

namespace BookingService_Application.Services;

public class OrderDetailService : IOrderDetailService
{
    private readonly IOrderDetailRepository _orderDetailRepository;

    public OrderDetailService(IOrderDetailRepository orderDetailRepository)
    {
        _orderDetailRepository = orderDetailRepository;
    }

    public async Task<ApiResponse<OrderDetailDto>> GetOrderDetailByIdAsync(Guid orderDetailId)
    {
        var orderDetail = await _orderDetailRepository.GetOrderDetailByIdAsync(orderDetailId);
        if (orderDetail == null)
            return ApiResponse<OrderDetailDto>.Fail(404, "Order detail not found");

        var orderDetailDto = orderDetail.Adapt<OrderDetailDto>();
        return ApiResponse<OrderDetailDto>.Success(200, "Order detail retrieved successfully", orderDetailDto);
    }

    public async Task<ApiResponse<IEnumerable<OrderDetailDto>>> GetOrderDetailsByOrderIdAsync(Guid orderId)
    {
        var orderDetails = await _orderDetailRepository.GetOrderDetailsByOrderIdAsync(orderId);
        var orderDetailDtos = orderDetails.Adapt<IEnumerable<OrderDetailDto>>();
        return ApiResponse<IEnumerable<OrderDetailDto>>.Success(200, "Order details retrieved successfully", orderDetailDtos);
    }
}
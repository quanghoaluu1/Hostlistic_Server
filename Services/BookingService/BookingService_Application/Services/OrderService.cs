using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using Mapster;

namespace BookingService_Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return ApiResponse<OrderDto>.Fail(404, "Order not found");

        var orderDto = order.Adapt<OrderDto>();
        return ApiResponse<OrderDto>.Success(200, "Order retrieved successfully", orderDto);
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByPayOsCodeAsync(long orderCode)
    {
        var order = await _orderRepository.GetOrderByOrderCodeAsync(orderCode);
        if (order is null)
            return ApiResponse<OrderDto>.Fail(404, "Order not found");
        var orderDto = order.Adapt<OrderDto>();
        return ApiResponse<OrderDto>.Success(200, "Order retrieved successfully", orderDto);
    }

    public async Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersByEventIdAsync(Guid eventId)
    {
        var orders = await _orderRepository.GetOrdersByEventIdAsync(eventId);
        var orderDtos = orders.Adapt<IEnumerable<OrderDto>>();
        return ApiResponse<IEnumerable<OrderDto>>.Success(200, "Orders retrieved successfully", orderDtos);
    }

    public async Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersByUserIdAsync(Guid userId)
    {
        var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
        var orderDtos = orders.Adapt<IEnumerable<OrderDto>>();
        return ApiResponse<IEnumerable<OrderDto>>.Success(200, "Orders retrieved successfully", orderDtos);
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderRequest request)
    {
        if (request.OrderDetails == null || !request.OrderDetails.Any())
            return ApiResponse<OrderDto>.Fail(400, "Order must have at least one order detail");

        var order = request.Adapt<Order>();
        order.Status = BookingService_Domain.Enum.OrderStatus.Pending;
        order.OrderDetails.Clear();

        // Create order details
        foreach (var detailRequest in request.OrderDetails)
        {
            var orderDetail = detailRequest.Adapt<OrderDetail>();
            orderDetail.OrderId = order.Id;
            order.OrderDetails.Add(orderDetail);
        }

        await _orderRepository.AddOrderAsync(order);
        await _orderRepository.SaveChangesAsync();

        var orderDto = order.Adapt<OrderDto>();
        return ApiResponse<OrderDto>.Success(201, "Order created successfully", orderDto);
    }

    public async Task<ApiResponse<OrderDto>> UpdateOrderAsync(Guid orderId, UpdateOrderRequest request)
    {
        var existingOrder = await _orderRepository.GetOrderByIdAsync(orderId);
        if (existingOrder == null)
            return ApiResponse<OrderDto>.Fail(404, "Order not found");

        // Update properties
        existingOrder.Status = request.Status;
        existingOrder.Notes = request.Notes;
        if (request.OrderCode is not null)
            existingOrder.OrderCode = request.OrderCode;

        await _orderRepository.UpdateOrderAsync(existingOrder);
        await _orderRepository.SaveChangesAsync();

        var orderDto = existingOrder.Adapt<OrderDto>();
        return ApiResponse<OrderDto>.Success(200, "Order updated successfully", orderDto);
    }

    public async Task<ApiResponse<bool>> DeleteOrderAsync(Guid orderId)
    {
        var exists = await _orderRepository.OrderExistsAsync(orderId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Order not found");

        var deleted = await _orderRepository.DeleteOrderAsync(orderId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete order");

        await _orderRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Order deleted successfully", true);
    }
}
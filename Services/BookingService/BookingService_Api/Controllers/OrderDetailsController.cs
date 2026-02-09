using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderDetailsController : ControllerBase
{
    private readonly IOrderDetailService _orderDetailService;

    public OrderDetailsController(IOrderDetailService orderDetailService)
    {
        _orderDetailService = orderDetailService;
    }

    [HttpGet("{orderDetailId:guid}")]
    public async Task<IActionResult> GetOrderDetailById(Guid orderDetailId)
    {
        var result = await _orderDetailService.GetOrderDetailByIdAsync(orderDetailId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetOrderDetailsByOrderId(Guid orderId)
    {
        var result = await _orderDetailService.GetOrderDetailsByOrderIdAsync(orderId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
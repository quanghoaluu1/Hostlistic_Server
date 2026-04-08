using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly ITicketPurchaseService _purchaseService;
        private readonly IPaymentMethodService _paymentMethodService;

        public PurchaseController(ITicketPurchaseService purchaseService, IPaymentMethodService paymentMethodService)
        {
            _purchaseService = purchaseService;
            _paymentMethodService = paymentMethodService;
        }

        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] InventoryCheckRequest request)
        {
            var result = await _purchaseService.CheckTicketAvailabilityAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("tickets")]
        public async Task<IActionResult> PurchaseTickets([FromBody] PurchaseTicketRequest request)
        {
            var result = await _purchaseService.PurchaseTicketsAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("payos")]
        public async Task<IActionResult> PurchasePayOs([FromBody] PurchaseTicketRequest request)
        {
            var result = await _purchaseService.InitiatePayOsPurchaseAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("payment-options")]
        public async Task<IActionResult> GetPaymentOptions([FromBody] GetPaymentOptionsRequest request)
        {
            var result = await _paymentMethodService.GetPaymentOptionsAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("free")]
        public async Task<IActionResult> PurchaseFreeTickets([FromBody] FreeTicketPurchaseRequest request)
        {
            var result = await _purchaseService.PurchaseFreeTicketsAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }
    }
}

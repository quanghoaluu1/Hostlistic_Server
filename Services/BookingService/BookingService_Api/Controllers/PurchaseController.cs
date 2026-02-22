using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly ITicketPurchaseService _purchaseService;

        public PurchaseController(ITicketPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
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
    }
}

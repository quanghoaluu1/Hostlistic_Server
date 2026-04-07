using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Obsolete]
    public class VenueController : ControllerBase
    {
        private readonly IVenueService _venueService;
        public VenueController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVenueAsync(Guid eventId, [FromBody] CreateVenueRequest createVenueDto)
        {
            var result = await _venueService.CreateAsync(eventId, createVenueDto);
            return Ok(result);
        }

        //[HttpGet("{id:guid}")]
        //public async Task<IActionResult> GetVenueByIdAsync(Guid id, Guid eventId)
        //{
        //    var result = await _venueService.GetByIdAsync(id);
        //    return Ok(result);
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAllVenuesAsync()
        //{
        //    var result = await _venueService.GetAllVenuesAsync();
        //    return Ok(result);
        //}

        //[HttpPut("{id:guid}")]
        //public async Task<IActionResult> UpdateVenueAsync(Guid id, [FromBody] CreateVenueDto updateVenueDto)
        //{
        //    var result = await _venueService.UpdateVenueAsync(id, updateVenueDto);
        //    return Ok(result);
        //}

        //[HttpDelete("{id:guid}")]
        //public async Task<IActionResult> DeleteVenueAsync(Guid id)
        //{
        //    var result = await _venueService.DeleteVenueAsync(id);
        //    return Ok(result);
        //}
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetVenueDashboardAsync(Guid? eventId = null)
        {
            var result = await _venueService.GetDashboardAsync(eventId);
            return Ok(result);
        }
    }
}

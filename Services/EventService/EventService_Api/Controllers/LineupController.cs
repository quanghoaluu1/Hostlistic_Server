using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineupController : ControllerBase
    {
        private readonly ILineupService _lineupService;
        public LineupController(ILineupService lineupService)
        {
            _lineupService = lineupService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateLineup([FromBody] CreateLineupsRequest request)
        {
            var result = await _lineupService.CreateLineupAsync(request);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("lineup/{eventId:guid}")]
        public async Task<IActionResult> GetLineupsByEventId(Guid eventId)
        {
            var result = await _lineupService.GetLineupsByEventIdAsync(eventId);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{lineupId:guid}")]
        public async Task<IActionResult> GetLineupById(Guid lineupId)
        {
            var result = await _lineupService.GetLineupById(lineupId);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLineups()
        {
            var result = await _lineupService.GetAllLineups();
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{lineupId:guid}")]
        public async Task<IActionResult> UpdateLineup(Guid lineupId, [FromBody] LineupDto request)
        {
            if (lineupId != request.Id)
            {
                return BadRequest();
            }
            var result = await _lineupService.UpdateLineupAsync(request);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{lineupId:guid}")]
        public async Task<IActionResult> DeleteLineup(Guid lineupId)
        {
            var result = await _lineupService.DeleteLineupAsync(lineupId);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}

using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PollController : ControllerBase
    {
        private readonly IPollService _pollService;

        public PollController(IPollService pollService)
        {
            _pollService = pollService;
        }
        [HttpPost]
        public async Task<IActionResult> AddPollAsync([FromBody] CreatePollRequest request)
        {
            var result = await _pollService.AddPollAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("{pollId:guid}")]
        public async Task<IActionResult> GetPollByIdAsync(Guid pollId)
        {
            var result = await _pollService.GetPollByIdAsync(pollId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("polls/{sessionId:guid}")]
        public async Task<IActionResult> GetPollsBySessionIdAsync(Guid sessionId)
        {
            var result = await _pollService.GetPollsBySessionIdAsync(sessionId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPut("{pollId:guid}")]
        public async Task<IActionResult> UpdatePollAsync(Guid pollId, [FromBody] UpdatePollRequest request)
        {
            var result = await _pollService.UpdatePollAsync(pollId, request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpDelete("{pollId:guid}")]
        public async Task<IActionResult> DeletePollAsync(Guid pollId)
        {
            var result = await _pollService.DeletePollAsync(pollId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}

using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentController : ControllerBase
    {
        private readonly ITalentService _talentService;
        public TalentController(ITalentService talentService)
        {
            _talentService = talentService;
        }
        // Controller actions go here
        [HttpGet("{talentId:guid}")]
        public async Task<IActionResult> GetTalentById(Guid talentId)
        {
            var result = await _talentService.GetTalentByIdAsync(talentId);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTalents()
        {
            var result = await _talentService.GetAllTalentsAsync();
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTalent([FromBody] CreateTalentDto request)
        {
            var result = await _talentService.CreateTalentAsync(request);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{talentId:guid}")]
        public async Task<IActionResult> UpdateTalent(Guid talentId, [FromBody] UpdateTalentDto request)
        {
            var result = await _talentService.UpdateTalentAsync(talentId, request);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
        [HttpDelete("{talentId:guid}")]
        public async Task<IActionResult> DeleteTalent(Guid talentId)
        {
            var result = await _talentService.DeleteTalentAsync(talentId);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}

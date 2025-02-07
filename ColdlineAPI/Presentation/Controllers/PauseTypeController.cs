using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PauseTypeController : ControllerBase
    {
        private readonly IPauseTypeService _pauseTypeService;

        public PauseTypeController(IPauseTypeService PauseTypeService)
        {
            _pauseTypeService = PauseTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PauseType>>> GetAllPauseTypes()
        {
            return Ok(await _pauseTypeService.GetAllPauseTypesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PauseType>> GetPauseTypeById(string id)
        {
            var PauseType = await _pauseTypeService.GetPauseTypeByIdAsync(id);
            return PauseType != null ? Ok(PauseType) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PauseType>> CreatePauseType([FromBody] PauseType PauseType)
        {
            var createdPauseType = await _pauseTypeService.CreatePauseTypeAsync(PauseType);
            return CreatedAtAction(nameof(GetPauseTypeById), new { id = createdPauseType.Id }, createdPauseType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePauseType(string id, [FromBody] PauseType PauseType)
        {
            var updated = await _pauseTypeService.UpdatePauseTypeAsync(id, PauseType);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePauseType(string id)
        {
            var deleted = await _pauseTypeService.DeletePauseTypeAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

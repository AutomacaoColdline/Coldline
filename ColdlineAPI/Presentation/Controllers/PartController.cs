using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartController(IPartService PartService)
        {
            _partService = PartService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Part>>> GetAllParts()
        {
            return Ok(await _partService.GetAllPartsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Part>> GetPartById(string id)
        {
            var Part = await _partService.GetPartByIdAsync(id);
            return Part != null ? Ok(Part) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Part>> CreatePart([FromBody] Part Part)
        {
            var createdPart = await _partService.CreatePartAsync(Part);
            return CreatedAtAction(nameof(GetPartById), new { id = createdPart.Id }, createdPart);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePart(string id, [FromBody] Part Part)
        {
            var updated = await _partService.UpdatePartAsync(id, Part);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePart(string id)
        {
            var deleted = await _partService.DeletePartAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

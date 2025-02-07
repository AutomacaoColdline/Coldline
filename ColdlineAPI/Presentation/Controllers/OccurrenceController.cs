using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OccurrenceController : ControllerBase
    {
        private readonly IOccurrenceService _occurrenceService;

        public OccurrenceController(IOccurrenceService OccurrenceService)
        {
            _occurrenceService = OccurrenceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Occurrence>>> GetAllOccurrences()
        {
            return Ok(await _occurrenceService.GetAllOccurrencesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Occurrence>> GetOccurrenceById(string id)
        {
            var Occurrence = await _occurrenceService.GetOccurrenceByIdAsync(id);
            return Occurrence != null ? Ok(Occurrence) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Occurrence>> CreateOccurrence([FromBody] Occurrence Occurrence)
        {
            var createdOccurrence = await _occurrenceService.CreateOccurrenceAsync(Occurrence);
            return CreatedAtAction(nameof(GetOccurrenceById), new { id = createdOccurrence.Id }, createdOccurrence);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOccurrence(string id, [FromBody] Occurrence Occurrence)
        {
            var updated = await _occurrenceService.UpdateOccurrenceAsync(id, Occurrence);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOccurrence(string id)
        {
            var deleted = await _occurrenceService.DeleteOccurrenceAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

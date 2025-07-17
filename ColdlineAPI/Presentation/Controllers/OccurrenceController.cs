using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Application.DTOs;
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

        public OccurrenceController(IOccurrenceService occurrenceService)
        {
            _occurrenceService = occurrenceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Occurrence>>> GetAllOccurrences()
        {
            var occurrences = await _occurrenceService.GetAllOccurrencesAsync();
            return Ok(occurrences);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Occurrence>> GetOccurrenceById(string id)
        {
            var occurrence = await _occurrenceService.GetOccurrenceByIdAsync(id);
            return occurrence != null ? Ok(occurrence) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Occurrence>> CreateOccurrence([FromBody] Occurrence occurrence)
        {
            var createdOccurrence = await _occurrenceService.CreateOccurrenceAsync(occurrence);
            return CreatedAtAction(nameof(GetOccurrenceById), new { id = createdOccurrence.Id }, createdOccurrence);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOccurrence(string id, [FromBody] Occurrence occurrence)
        {
            var updated = await _occurrenceService.UpdateOccurrenceAsync(id, occurrence);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOccurrence(string id)
        {
            var deleted = await _occurrenceService.DeleteOccurrenceAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("start-occurrence")]
        public async Task<ActionResult<Occurrence>> StartOccurrence([FromBody] StartOccurrenceRequest request)
        {
            try
            {
                var occurrence = await _occurrenceService.StartOccurrenceAsync(request);
                return CreatedAtAction(nameof(GetOccurrenceById), new { id = occurrence.Id }, occurrence);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("end-occurrence/{id}")]
        public async Task<IActionResult> EndOccurrence(string id)
        {
            try
            {
                var result = await _occurrenceService.EndOccurrenceAsync(id);
                return result
                    ? Ok(new { message = "Ocorrência finalizada com sucesso." })
                    : BadRequest(new { message = "Falha ao finalizar a ocorrência." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("chart/by-date")]
        public async Task<IActionResult> GetChartByDate([FromBody] DateRangeRequest range)
        {
            var result = await _occurrenceService.GetOccurrenceCountByDateAsync(range.StartDate, range.EndDate);
            return Ok(result);
        }

        [HttpPost("chart/by-user")]
        public async Task<IActionResult> GetChartByUser([FromBody] DateRangeRequest range)
        {
            var result = await _occurrenceService.GetOccurrenceCountByUserAsync(range.StartDate, range.EndDate);
            return Ok(result);
        }
    }
}

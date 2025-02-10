using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QualityController : ControllerBase
    {
        private readonly IQualityService _qualityService;

        public QualityController(IQualityService QualityService)
        {
            _qualityService = QualityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quality>>> GetAllQualitys()
        {
            return Ok(await _qualityService.GetAllQualitysAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Quality>> GetQualityById(string id)
        {
            var Quality = await _qualityService.GetQualityByIdAsync(id);
            return Quality != null ? Ok(Quality) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Quality>> CreateQuality([FromBody] Quality Quality)
        {
            var createdQuality = await _qualityService.CreateQualityAsync(Quality);
            return CreatedAtAction(nameof(GetQualityById), new { id = createdQuality.Id }, createdQuality);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuality(string id, [FromBody] Quality Quality)
        {
            var updated = await _qualityService.UpdateQualityAsync(id, Quality);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuality(string id)
        {
            var deleted = await _qualityService.DeleteQualityAsync(id);
            return deleted ? NoContent() : NotFound();
        }
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Quality>>> SearchQuality([FromBody] QualityFilter filter)
        {
            var results = await _qualityService.SearchQualityAsync(filter);
            return Ok(results);
        }
    }
}

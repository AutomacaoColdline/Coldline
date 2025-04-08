using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
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

        public QualityController(IQualityService qualityService)
        {
            _qualityService = qualityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quality>>> GetAllQualitys()
        {
            var result = await _qualityService.GetAllQualitysAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Quality>> GetQualityById(string id)
        {
            var quality = await _qualityService.GetQualityByIdAsync(id);
            return quality != null ? Ok(quality) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Quality>> CreateQuality([FromBody] Quality quality)
        {
            var created = await _qualityService.CreateQualityAsync(quality);
            return CreatedAtAction(nameof(GetQualityById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuality(string id, [FromBody] Quality quality)
        {
            var updated = await _qualityService.UpdateQualityAsync(id, quality);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuality(string id)
        {
            var deleted = await _qualityService.DeleteQualityAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchQuality([FromBody] QualityFilter filter)
        {
            var (items, totalCount) = await _qualityService.SearchQualityAsync(filter);
            return Ok(new { Items = items, TotalCount = totalCount });
        }

    }
}

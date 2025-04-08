using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DefectController : ControllerBase
    {
        private readonly IDefectService _defectService;

        public DefectController(IDefectService defectService)
        {
            _defectService = defectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Defect>>> GetAllDefects()
        {
            var defects = await _defectService.GetAllDefectsAsync();
            return Ok(defects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Defect>> GetDefectById(string id)
        {
            var defect = await _defectService.GetDefectByIdAsync(id);
            return defect != null ? Ok(defect) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Defect>> CreateDefect([FromBody] Defect defect)
        {
            var created = await _defectService.CreateDefectAsync(defect);
            return CreatedAtAction(nameof(GetDefectById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDefect(string id, [FromBody] Defect defect)
        {
            var updated = await _defectService.UpdateDefectAsync(id, defect);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDefect(string id)
        {
            try
            {
                var deleted = await _defectService.DeleteDefectAsync(id);
                return deleted ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

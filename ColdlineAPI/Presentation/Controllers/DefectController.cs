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

        public DefectController(IDefectService DefectService)
        {
            _defectService = DefectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Defect>>> GetAllDefects()
        {
            return Ok(await _defectService.GetAllDefectsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Defect>> GetDefectById(string id)
        {
            var Defect = await _defectService.GetDefectByIdAsync(id);
            return Defect != null ? Ok(Defect) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Defect>> CreateDefect([FromBody] Defect Defect)
        {
            var createdDefect = await _defectService.CreateDefectAsync(Defect);
            return CreatedAtAction(nameof(GetDefectById), new { id = createdDefect.Id }, createdDefect);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDefect(string id, [FromBody] Defect Defect)
        {
            var updated = await _defectService.UpdateDefectAsync(id, Defect);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDefect(string id)
        {
            var deleted = await _defectService.DeleteDefectAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

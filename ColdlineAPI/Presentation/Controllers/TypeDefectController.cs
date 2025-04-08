using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TypeDefectController : ControllerBase
    {
        private readonly ITypeDefectService _typeDefectService;

        public TypeDefectController(ITypeDefectService typeDefectService)
        {
            _typeDefectService = typeDefectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeDefect>>> GetAllTypeDefects()
        {
            return Ok(await _typeDefectService.GetAllTypeDefectsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TypeDefect>> GetTypeDefectById(string id)
        {
            var typeDefect = await _typeDefectService.GetTypeDefectByIdAsync(id);
            return typeDefect != null ? Ok(typeDefect) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<TypeDefect>> CreateTypeDefect([FromBody] TypeDefect typeDefect)
        {
            var created = await _typeDefectService.CreateTypeDefectAsync(typeDefect);
            return CreatedAtAction(nameof(GetTypeDefectById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTypeDefect(string id, [FromBody] TypeDefect typeDefect)
        {
            var updated = await _typeDefectService.UpdateTypeDefectAsync(id, typeDefect);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTypeDefect(string id)
        {
            var deleted = await _typeDefectService.DeleteTypeDefectAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

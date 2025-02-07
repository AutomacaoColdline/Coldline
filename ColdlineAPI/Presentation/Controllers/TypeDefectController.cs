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

        public TypeDefectController(ITypeDefectService TypeDefectService)
        {
            _typeDefectService = TypeDefectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeDefect>>> GetAllTypeDefects()
        {
            return Ok(await _typeDefectService.GetAllTypeDefectsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TypeDefect>> GetTypeDefectById(string id)
        {
            var TypeDefect = await _typeDefectService.GetTypeDefectByIdAsync(id);
            return TypeDefect != null ? Ok(TypeDefect) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<TypeDefect>> CreateTypeDefect([FromBody] TypeDefect TypeDefect)
        {
            var createdTypeDefect = await _typeDefectService.CreateTypeDefectAsync(TypeDefect);
            return CreatedAtAction(nameof(GetTypeDefectById), new { id = createdTypeDefect.Id }, createdTypeDefect);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTypeDefect(string id, [FromBody] TypeDefect TypeDefect)
        {
            var updated = await _typeDefectService.UpdateTypeDefectAsync(id, TypeDefect);
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

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

        public PartController(IPartService partService)
        {
            _partService = partService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllParts()
        {
            var items = await _partService.GetAllPartsAsync();
            return Ok(items);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetPartById(string id)
        {
            var part = await _partService.GetPartByIdAsync(id);
            return part != null ? Ok(part) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePart([FromBody] Part part)
        {
            var created = await _partService.CreatePartAsync(part);
            return CreatedAtAction(nameof(GetPartById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePart(string id, [FromBody] Part part)
        {
            var updated = await _partService.UpdatePartAsync(id, part);
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

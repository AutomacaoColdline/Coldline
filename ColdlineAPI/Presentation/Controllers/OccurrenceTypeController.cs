using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OccurrenceTypeController : ControllerBase
    {
        private readonly IOccurrenceTypeService _occurrenceTypeService;

        public OccurrenceTypeController(IOccurrenceTypeService OccurrenceTypeService)
        {
            _occurrenceTypeService = OccurrenceTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOccurrenceTypes()
        {
            var items = await _occurrenceTypeService.GetAllOccurrenceTypesAsync();
            return Ok(items);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetOccurrenceTypeById(string id)
        {
            var occurrenceType = await _occurrenceTypeService.GetOccurrenceTypeByIdAsync(id);
            return occurrenceType != null ? Ok(occurrenceType) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOccurrenceType([FromBody] OccurrenceType occurrenceType)
        {
            var created = await _occurrenceTypeService.CreateOccurrenceTypeAsync(occurrenceType);
            return CreatedAtAction(nameof(GetOccurrenceTypeById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOccurrenceType(string id, [FromBody] OccurrenceType occurrenceType)
        {
            var updated = await _occurrenceTypeService.UpdateOccurrenceTypeAsync(id, occurrenceType);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOccurrenceType(string id)
        {
            var deleted = await _occurrenceTypeService.DeleteOccurrenceTypeAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<OccurrenceType>>> SearchProcess([FromQuery] OccurrenceTypeFilter filter)
        {
            var result = await _occurrenceTypeService.SearchOccurrenceTypesAsync(filter);
            return Ok(result);
        }
    }
}

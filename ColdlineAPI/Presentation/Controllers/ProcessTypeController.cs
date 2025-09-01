using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessTypeController : ControllerBase
    {
        private readonly IProcessTypeService _processTypeService;

        public ProcessTypeController(IProcessTypeService processTypeService)
        {
            _processTypeService = processTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcessType>>> GetAllProcessTypes()
        {
            var items = await _processTypeService.GetAllProcessTypesAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProcessType>> GetProcessTypeById(string id)
        {
            var item = await _processTypeService.GetProcessTypeByIdAsync(id);
            return item != null ? Ok(item) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<ProcessType>> CreateProcessType([FromBody] ProcessType processType)
        {
            var created = await _processTypeService.CreateProcessTypeAsync(processType);
            return CreatedAtAction(nameof(GetProcessTypeById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProcessType(string id, [FromBody] ProcessType processType)
        {
            var updated = await _processTypeService.UpdateProcessTypeAsync(id, processType);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProcessType(string id)
        {
            var deleted = await _processTypeService.DeleteProcessTypeAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<ProcessType>>> Search([FromQuery] ProcessTypeFilter filter)
        {
            var result = await _processTypeService.SearchProcessTypesAsync(filter);
            return Ok(result);
        }
    }
}

using ColdlineAPI.Application.Interfaces;
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

        public ProcessTypeController(IProcessTypeService ProcessTypeService)
        {
            _processTypeService = ProcessTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcessType>>> GetAllProcessTypes()
        {
            return Ok(await _processTypeService.GetAllProcessTypesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProcessType>> GetProcessTypeById(string id)
        {
            var ProcessType = await _processTypeService.GetProcessTypeByIdAsync(id);
            return ProcessType != null ? Ok(ProcessType) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<ProcessType>> CreateProcessType([FromBody] ProcessType ProcessType)
        {
            var createdProcessType = await _processTypeService.CreateProcessTypeAsync(ProcessType);
            return CreatedAtAction(nameof(GetProcessTypeById), new { id = createdProcessType.Id }, createdProcessType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProcessType(string id, [FromBody] ProcessType ProcessType)
        {
            var updated = await _processTypeService.UpdateProcessTypeAsync(id, ProcessType);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProcessType(string id)
        {
            var deleted = await _processTypeService.DeleteProcessTypeAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

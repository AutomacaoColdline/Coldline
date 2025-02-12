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
    public class ProcessController : ControllerBase
    {
        private readonly IProcessService _processService;

        public ProcessController(IProcessService processService)
        {
            _processService = processService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Process>>> GetAllProcesses()
        {
            return Ok(await _processService.GetAllProcesssAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Process>> GetProcessById(string id)
        {
            var process = await _processService.GetProcessByIdAsync(id);
            return process != null ? Ok(process) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Process>> CreateProcess([FromBody] Process process)
        {
            var createdProcess = await _processService.CreateProcessAsync(process);
            return CreatedAtAction(nameof(GetProcessById), new { id = createdProcess.Id }, createdProcess);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProcess(string id, [FromBody] Process process)
        {
            var updated = await _processService.UpdateProcessAsync(id, process);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProcess(string id)
        {
            var deleted = await _processService.DeleteProcessAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Process>>> SearchProcesses([FromBody] ProcessFilter filter)
        {
            var processes = await _processService.SearchProcessAsync(filter);
            return Ok(processes);
        }
    }
}

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

        public ProcessController(IProcessService ProcessService)
        {
            _processService = ProcessService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Process>>> GetAllProcesss()
        {
            return Ok(await _processService.GetAllProcesssAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Process>> GetProcessById(string id)
        {
            var Process = await _processService.GetProcessByIdAsync(id);
            return Process != null ? Ok(Process) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Process>> CreateProcess([FromBody] Process Process)
        {
            var createdProcess = await _processService.CreateProcessAsync(Process);
            return CreatedAtAction(nameof(GetProcessById), new { id = createdProcess.Id }, createdProcess);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProcess(string id, [FromBody] Process Process)
        {
            var updated = await _processService.UpdateProcessAsync(id, Process);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProcess(string id)
        {
            var deleted = await _processService.DeleteProcessAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.DTOs;
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
            return Ok(await _processService.GetAllProcessesAsync());
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
            if (process == null || string.IsNullOrEmpty(id))
            {
                return BadRequest(new { Message = "Os dados do processo são inválidos." });
            }

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
        [HttpPost("start-process")]
        public async Task<IActionResult> StartProcess([FromBody] StartProcessRequest request)
        {
            try
            {
                var process = await _processService.StartProcessAsync(request.IdentificationNumber, request.ProcessTypeId, request.MachineId, request.PreIndustrialization, request.ReWork);
                return Ok(process);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("end-process/{id}")]
        public async Task<IActionResult> EndProcess(string id)
        {
            try
            {
                var result = await _processService.EndProcessAsync(id);
                return result ? Ok(new { message = "Processo finalizado com sucesso." }) : BadRequest(new { message = "Falha ao finalizar o processo." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("statistics/{processId}/{processTypeId}")]
        public async Task<IActionResult> GetProcessStatistics(string processId, string processTypeId)
        {
            try
            {
                var statistics = await _processService.GetProcessStatisticsAsync(processId, processTypeId);
                return Ok(statistics);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("user-data/{userId}")]
        public async Task<ActionResult<UserProcessDetailsDto>> GetUserProcessData(string userId)
        {
            try
            {
                var result = await _processService.GetUserProcessDataAsync(userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

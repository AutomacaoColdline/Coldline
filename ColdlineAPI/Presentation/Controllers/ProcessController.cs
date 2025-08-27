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
                var process = await _processService.StartProcessAsync(request.IdentificationNumber, request.ProcessTypeId, request.MachineId, request.PreIndustrialization, request.ReWork, request.Prototype);
                return Ok(process);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpPost("end-process/{id}")]
        public async Task<IActionResult> EndProcess(string id, [FromQuery] bool finished, [FromBody] StartOccurrenceRequest? request)
        {
            if (!finished && request == null)
                return BadRequest(new { message = "RequestOccurrence é obrigatório quando finished=false." });

            try
            {
                var ok = await _processService.EndProcessAsync(id, finished, request!);

                if (!ok)
                    return BadRequest(new { message = "Falha ao processar a solicitação." });

                return Ok(new
                {
                    message = finished
                        ? "Processo finalizado com sucesso."
                        : "Ocorrência registrada com sucesso."
                });
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
        // Endpoint no ProcessController
        [HttpPost("chart/by-date")]
        public async Task<ActionResult<List<ProcessByDateDto>>> GetProcessChartData([FromBody] DateRangeRequest request)
        {
            if (request.StartDate > request.EndDate)
                return BadRequest("Data inicial não pode ser maior que a final.");

            var result = await _processService.GetProcessCountByStartDateAsync(request.StartDate, request.EndDate);
            return Ok(result);
        }

        [HttpPost("chart/by-type-and-date")]
        public async Task<IActionResult> GetChartByTypeAndDate([FromBody] DateRangeRequest request)
        {
            var result = await _processService.GetProcessCountByTypeAndDateAsync(request.StartDate, request.EndDate);
            return Ok(result);
        }

        [HttpPost("chart/by-user-and-date")]
        public async Task<ActionResult<List<ProcessUserChartDto>>> GetChartByUserAndDate([FromBody] DateRangeRequest request)
        {
            var result = await _processService.GetProcessCountByUserAsync(request.StartDate, request.EndDate);
            return Ok(result);
        }

        [HttpPost("chart/total-time-by-user")]
        public async Task<ActionResult<List<UserTotalProcessTimeDto>>> GetTotalProcessTimeByUser([FromBody] DateRangeRequest request)
        {
            if (request.StartDate > request.EndDate)
                return BadRequest("Data inicial não pode ser maior que a final.");

            var result = await _processService.GetTotalProcessTimeByUserAsync(request.StartDate, request.EndDate);
            return Ok(result);
        }

        [HttpPost("chart/total-time-by-type")]
        public async Task<ActionResult<List<ProcessTypeTotalTimeDto>>> GetTotalProcessTimeByType([FromBody] DateRangeRequest request)
        {
            if (request.StartDate > request.EndDate)
                return BadRequest("Data inicial não pode ser maior que a final.");

            var result = await _processService.GetTotalProcessTimeByProcessTypeAsync(request.StartDate, request.EndDate);
            return Ok(result);
        }
        [HttpPost("chart/individual-time-by-user/{userId}")]
        public async Task<ActionResult<List<IndividualUserProcessDto>>> GetIndividualProcessTimesByUser(string userId, [FromBody] IndividualUserProcessRequest request)
        {
            if (request.StartDate > request.EndDate)
                return BadRequest("Data inicial não pode ser maior que a final.");

            var result = await _processService.GetIndividualProcessTimesByUserAsync(userId, request.StartDate, request.EndDate, request.PreIndustrialization);
            return Ok(result);
        }
        [HttpPost("generate-excel-report")]
        public async Task<IActionResult> GenerateExcelReport([FromBody] DateRangeRequest request)
        {
            try
            {
                var fileBytes = await _processService.GenerateExcelReportAsync(request.StartDate, request.EndDate);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "relatorio-processos.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro ao gerar Excel: {ex.Message}" });
            }
        }


    }
}

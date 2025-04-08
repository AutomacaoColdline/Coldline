using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Domain.Enum;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineController : ControllerBase
    {
        private readonly IMachineService _machineService;

        public MachineController(IMachineService machineService)
        {
            _machineService = machineService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Machine>>> GetAllMachines()
        {
            return Ok(await _machineService.GetAllMachinesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Machine>> GetMachineById(string id)
        {
            var machine = await _machineService.GetMachineByIdAsync(id);
            return machine != null ? Ok(machine) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Machine>> CreateMachine([FromBody] Machine machine)
        {
            var createdMachine = await _machineService.CreateMachineAsync(machine);
            return CreatedAtAction(nameof(GetMachineById), new { id = createdMachine.Id }, createdMachine);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMachine(string id, [FromBody] Machine machine)
        {
            var updated = await _machineService.UpdateMachineAsync(id, machine);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachine(string id)
        {
            var deleted = await _machineService.DeleteMachineAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchMachines(
            [FromQuery] string? customerName,
            [FromQuery] string? identificationNumber,
            [FromQuery] string? phase,
            [FromQuery] string? voltage,
            [FromQuery] string? processId,
            [FromQuery] string? qualityId,
            [FromQuery] string? monitoringId,
            [FromQuery] string? machineTypeId,
            [FromQuery] int? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var filter = new MachineFilter
            {
                CustomerName = customerName,
                IdentificationNumber = identificationNumber,
                Phase = phase,
                Voltage = voltage,
                ProcessId = processId,
                QualityId = qualityId,
                MonitoringId = monitoringId,
                MachineTypeId = machineTypeId,
                Status = status.HasValue ? (MachineStatus?)status.Value : null,
                Page = page,
                PageSize = pageSize
            };

            var items = await _machineService.SearchMachinesAsync(filter);

            var response = new
            {
                pageNumber = page,
                pageSize = pageSize,
                totalCount = items.Count, // ou estimativa se implementar
                totalPages = (int)System.Math.Ceiling((double)items.Count / pageSize),
                items
            };

            return Ok(response);
        }
    }
}

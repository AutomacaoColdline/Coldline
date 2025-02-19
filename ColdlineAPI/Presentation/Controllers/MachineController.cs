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


        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Machine>>> SearchMachines([FromBody] MachineFilter filter)
        {
            var machines = await _machineService.SearchMachinesAsync(filter);
            return Ok(machines);
        }
    }
}

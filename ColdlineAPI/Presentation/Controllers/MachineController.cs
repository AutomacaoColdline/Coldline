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

        public MachineController(IMachineService MachineService)
        {
            _machineService = MachineService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Machine>>> GetAllMachines()
        {
            return Ok(await _machineService.GetAllMachinesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Machine>> GetMachineById(string id)
        {
            var Machine = await _machineService.GetMachineByIdAsync(id);
            return Machine != null ? Ok(Machine) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Machine>> CreateMachine([FromBody] Machine Machine)
        {
            var createdMachine = await _machineService.CreateMachineAsync(Machine);
            return CreatedAtAction(nameof(GetMachineById), new { id = createdMachine.Id }, createdMachine);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMachine(string id, [FromBody] Machine Machine)
        {
            var updated = await _machineService.UpdateMachineAsync(id, Machine);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachine(string id)
        {
            var deleted = await _machineService.DeleteMachineAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

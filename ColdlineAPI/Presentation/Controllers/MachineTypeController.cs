using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineTypeController : ControllerBase
    {
        private readonly IMachineTypeService _machineTypeService;

        public MachineTypeController(IMachineTypeService machineTypeService)
        {
            _machineTypeService = machineTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMachineTypes()
        {
            var result = await _machineTypeService.GetAllMachineTypesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMachineTypeById(string id)
        {
            var result = await _machineTypeService.GetMachineTypeByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMachineType([FromBody] MachineType machineType)
        {
            var created = await _machineTypeService.CreateMachineTypeAsync(machineType);
            return CreatedAtAction(nameof(GetMachineTypeById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMachineType(string id, [FromBody] MachineType machineType)
        {
            var success = await _machineTypeService.UpdateMachineTypeAsync(id, machineType);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachineType(string id)
        {
            var success = await _machineTypeService.DeleteMachineTypeAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}

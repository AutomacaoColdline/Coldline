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
        private readonly IMachineTypeService _defectService;

        public MachineTypeController(IMachineTypeService MachineTypeService)
        {
            _defectService = MachineTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MachineType>>> GetAllMachineTypes()
        {
            return Ok(await _defectService.GetAllMachineTypesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MachineType>> GetMachineTypeById(string id)
        {
            var MachineType = await _defectService.GetMachineTypeByIdAsync(id);
            return MachineType != null ? Ok(MachineType) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<MachineType>> CreateMachineType([FromBody] MachineType MachineType)
        {
            var createdMachineType = await _defectService.CreateMachineTypeAsync(MachineType);
            return CreatedAtAction(nameof(GetMachineTypeById), new { id = createdMachineType.Id }, createdMachineType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMachineType(string id, [FromBody] MachineType MachineType)
        {
            var updated = await _defectService.UpdateMachineTypeAsync(id, MachineType);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachineType(string id)
        {
            var deleted = await _defectService.DeleteMachineTypeAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

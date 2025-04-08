using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringTypeController : ControllerBase
    {
        private readonly IMonitoringTypeService _monitoringTypeService;

        public MonitoringTypeController(IMonitoringTypeService monitoringTypeService)
        {
            _monitoringTypeService = monitoringTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitoringType>>> GetAllMonitoringTypes()
        {
            return Ok(await _monitoringTypeService.GetAllMonitoringTypesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MonitoringType>> GetMonitoringTypeById(string id)
        {
            var result = await _monitoringTypeService.GetMonitoringTypeByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<MonitoringType>> CreateMonitoringType([FromBody] MonitoringType monitoringType)
        {
            var created = await _monitoringTypeService.CreateMonitoringTypeAsync(monitoringType);
            return CreatedAtAction(nameof(GetMonitoringTypeById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMonitoringType(string id, [FromBody] MonitoringType monitoringType)
        {
            var updated = await _monitoringTypeService.UpdateMonitoringTypeAsync(id, monitoringType);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMonitoringType(string id)
        {
            var deleted = await _monitoringTypeService.DeleteMonitoringTypeAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

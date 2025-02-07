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

        public MonitoringTypeController(IMonitoringTypeService MonitoringTypeService)
        {
            _monitoringTypeService = MonitoringTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitoringType>>> GetAllMonitoringTypes()
        {
            return Ok(await _monitoringTypeService.GetAllMonitoringTypesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MonitoringType>> GetMonitoringTypeById(string id)
        {
            var MonitoringType = await _monitoringTypeService.GetMonitoringTypeByIdAsync(id);
            return MonitoringType != null ? Ok(MonitoringType) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<MonitoringType>> CreateMonitoringType([FromBody] MonitoringType MonitoringType)
        {
            var createdMonitoringType = await _monitoringTypeService.CreateMonitoringTypeAsync(MonitoringType);
            return CreatedAtAction(nameof(GetMonitoringTypeById), new { id = createdMonitoringType.Id }, createdMonitoringType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMonitoringType(string id, [FromBody] MonitoringType MonitoringType)
        {
            var updated = await _monitoringTypeService.UpdateMonitoringTypeAsync(id, MonitoringType);
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

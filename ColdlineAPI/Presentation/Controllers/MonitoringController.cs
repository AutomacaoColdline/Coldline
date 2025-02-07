using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        private readonly IMonitoringService _monitoringService;

        public MonitoringController(IMonitoringService MonitoringService)
        {
            _monitoringService = MonitoringService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Monitoring>>> GetAllMonitorings()
        {
            return Ok(await _monitoringService.GetAllMonitoringsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Monitoring>> GetMonitoringById(string id)
        {
            var Monitoring = await _monitoringService.GetMonitoringByIdAsync(id);
            return Monitoring != null ? Ok(Monitoring) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Monitoring>> CreateMonitoring([FromBody] Monitoring Monitoring)
        {
            var createdMonitoring = await _monitoringService.CreateMonitoringAsync(Monitoring);
            return CreatedAtAction(nameof(GetMonitoringById), new { id = createdMonitoring.Id }, createdMonitoring);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMonitoring(string id, [FromBody] Monitoring Monitoring)
        {
            var updated = await _monitoringService.UpdateMonitoringAsync(id, Monitoring);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMonitoring(string id)
        {
            var deleted = await _monitoringService.DeleteMonitoringAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

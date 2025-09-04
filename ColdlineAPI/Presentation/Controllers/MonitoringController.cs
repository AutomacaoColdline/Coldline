using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Common;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Filters;
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

        public MonitoringController(IMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Monitoring>>> GetAll()
        {
            var monitorings = await _monitoringService.GetAllMonitoringsAsync();
            return Ok(monitorings);
        }
        [HttpGet("search")]
        [ProducesResponseType(typeof(PagedResult<Monitoring>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<Monitoring>>> Search([FromQuery] MonitoringFilter filter)
        {
            var result = await _monitoringService.SearchMonitoringsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Monitoring>> GetById(string id)
        {
            var monitoring = await _monitoringService.GetMonitoringByIdAsync(id);
            return monitoring != null ? Ok(monitoring) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Monitoring>> Create([FromBody] Monitoring monitoring)
        {
            var created = await _monitoringService.CreateMonitoringAsync(monitoring);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPost("batch")]
        public async Task<ActionResult<IEnumerable<Monitoring>>> CreateAll([FromBody] List<Monitoring> monitorings)
        {
            if (monitorings == null || monitorings.Count == 0)
                return BadRequest("A lista de monitoramentos está vazia.");

            var createdList = await _monitoringService.CreateAllMonitoringsAsync(monitorings);
            return Ok(createdList);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Monitoring monitoring)
        {
            var updated = await _monitoringService.UpdateMonitoringAsync(id, monitoring);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _monitoringService.DeleteMonitoringAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

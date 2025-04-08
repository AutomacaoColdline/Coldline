using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(string id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            return department != null ? Ok(department) : NotFound(new { Message = "Departamento não encontrado." });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] Department department)
        {
            var created = await _departmentService.CreateDepartmentAsync(department);
            return CreatedAtAction(nameof(GetDepartmentById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(string id, [FromBody] Department department)
        {
            var (success, message) = await _departmentService.UpdateDepartmentAsync(id, department);
            return success ? Ok(new { Message = message }) : BadRequest(new { Message = message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(string id)
        {
            var (success, message) = await _departmentService.DeleteDepartmentAsync(id);
            return success ? Ok(new { Message = message }) : BadRequest(new { Message = message });
        }
    }
}

using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTypeController : ControllerBase
    {
        private readonly IUserTypeService _userTypeService;

        public UserTypeController(IUserTypeService userTypeService)
        {
            _userTypeService = userTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserType>>> GetAllUserTypes()
        {
            return Ok(await _userTypeService.GetAllUserTypesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserType>> GetUserTypeById(string id)
        {
            var userType = await _userTypeService.GetUserTypeByIdAsync(id);
            return userType != null ? Ok(userType) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<UserType>> CreateUserType([FromBody] UserType userType)
        {
            var createdUserType = await _userTypeService.CreateUserTypeAsync(userType);
            return CreatedAtAction(nameof(GetUserTypeById), new { id = createdUserType.Id }, createdUserType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserType(string id, [FromBody] UserType userType)
        {
            var updated = await _userTypeService.UpdateUserTypeAsync(id, userType);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserType(string id)
        {
            var deleted = await _userTypeService.DeleteUserTypeAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}

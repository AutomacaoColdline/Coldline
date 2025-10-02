using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
using ColdlineAPI.Application.Filters;
using ColdlineAPI.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly string _uploadsPath;

        public UserController(IUserService userService)
        {
            _userService = userService;
            _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user != null ? Ok(user) : NotFound(new { Message = "Usuário não encontrado!" });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            ModelState.Remove("Id");
            try
            {
                var createdUser = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            var (updated, message) = await _userService.UpdateUserAsync(id, user);
            return updated ? Ok(new { Message = "Usuário atualizado com sucesso!" }) : BadRequest(new { Message = message });
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            return deleted ? NoContent() : NotFound(new { Message = "Usuário não encontrado para exclusão!" });
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<User>>> SearchUsers([FromQuery] UserFilter filter)
        {
            var result = await _userService.SearchUsersAsync(filter);
            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.AuthenticateUserAsync(request.Email, request.Password);
            if (user == null) return Unauthorized(new { Message = "E-mail ou senha inválidos!" });
            var token = _userService.GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            bool success = await _userService.ForgotPasswordAsync(request.Email);
            return success ? Ok(new { Message = "Se este e-mail estiver cadastrado, a senha foi enviada." }) : NotFound(new { Message = "E-mail não encontrado!" });
        }

        [HttpGet("identification/{identificationNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByIdentificationNumber(string identificationNumber)
        {
            var user = await _userService.GetUserByIdentificationNumberAsync(identificationNumber);
            return user != null ? Ok(user) : NotFound(new { Message = "Usuário não encontrado!" });
        }

        [HttpPost("change-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            bool success = await _userService.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);
            return success ? Ok(new { Message = "Senha alterada com sucesso!" }) : BadRequest(new { Message = "Senha antiga incorreta ou usuário não encontrado!" });
        }

        [HttpGet("uploads/{fileName}")]
        [AllowAnonymous]
        public IActionResult GetStaticFile(string fileName)
        {
            var filePath = Path.Combine(_uploadsPath, fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound(new { Message = "Arquivo não encontrado!" });

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(filePath, contentType);
        }

        [HttpPost("upload-image")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadImage(IFormFile? file, [FromQuery] string? oldFileName, [FromQuery] string newFileName)
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var oldFilePath = string.IsNullOrWhiteSpace(oldFileName) ? null : Path.Combine(uploadsFolder, $"{oldFileName}.png");
                var newFilePath = Path.Combine(uploadsFolder, $"{newFileName}.png");

                if (file != null && string.IsNullOrWhiteSpace(oldFileName))
                {
                    using var stream = new FileStream(newFilePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    return Ok(new { url = $"/uploads/{newFileName}.png" });
                }

                if (file == null && !string.IsNullOrWhiteSpace(oldFileName))
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Move(oldFilePath, newFilePath);
                        return Ok(new { url = $"/uploads/{newFileName}.png" });
                    }
                    return NotFound(new { Message = "Imagem anterior não encontrada para renomeação." });
                }

                if (file != null && !string.IsNullOrWhiteSpace(oldFileName) && oldFileName == newFileName)
                {
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    using var stream = new FileStream(newFilePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    return Ok(new { url = $"/uploads/{newFileName}.png" });
                }

                if (file != null && !string.IsNullOrWhiteSpace(oldFileName) && oldFileName != newFileName)
                {
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    using var stream = new FileStream(newFilePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    return Ok(new { url = $"/uploads/{newFileName}.png" });
                }

                return BadRequest(new { Message = "Nenhuma operação foi realizada. Verifique os parâmetros enviados." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao processar a imagem.", Error = ex.Message });
            }
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel()
        {
            var bytes = await _userService.GenerateExcelWithSampleDataAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "usuarios.xlsx");
        }

        [HttpGet("export-pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var bytes = await _userService.GeneratePdfWithAgeChartAsync();
            return File(bytes, "application/pdf", "grafico-usuarios.pdf");
        }
    }
}
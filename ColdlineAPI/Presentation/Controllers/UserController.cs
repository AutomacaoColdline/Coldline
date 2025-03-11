using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Domain.Entities;
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
        private readonly string _staticFilesPath;
        private readonly string _uploadsPath;

        public UserController(IUserService userService)
        {
            _userService = userService;
            _staticFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "../../ColdlineWeb/wwwroot");
            _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "../../ColdlineWeb/wwwroot/uploads");
        }

        /// <summary>
        /// Retorna todos os usuários cadastrados.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retorna um usuário pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user != null ? Ok(user) : NotFound(new { Message = "Usuário não encontrado!" });
        }

        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
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

        /// <summary>
        /// Atualiza um usuário pelo ID.
        /// </summary>
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            var updated = await _userService.UpdateUserAsync(id, user);
            return updated ? NoContent() : NotFound(new { Message = "Usuário não encontrado para atualização!" });
        }

        /// <summary>
        /// Exclui um usuário pelo ID.
        /// </summary>
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            return deleted ? NoContent() : NotFound(new { Message = "Usuário não encontrado para exclusão!" });
        }
        
        /// <summary>
        /// Busca usuários com filtros opcionais.
        /// </summary>
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string? name,
            [FromQuery] string? email,
            [FromQuery] string? departmentId,
            [FromQuery] string? userTypeId)
        {
            var users = await _userService.SearchUsersAsync(name, email, departmentId, userTypeId);
            return Ok(users);
        }

        /// <summary>
        /// Realiza login e retorna um token JWT.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) 
        {
            var user = await _userService.AuthenticateUserAsync(request.Email, request.Password);

            if (user == null)
            {
                return Unauthorized(new { Message = "E-mail ou senha inválidos!" });
            }

            var token = _userService.GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            bool success = await _userService.ForgotPasswordAsync(request.Email);

            if (!success)
            {
                return NotFound(new { Message = "E-mail não encontrado!" });
            }

            return Ok(new { Message = "Se este e-mail estiver cadastrado, a senha foi enviada." });
        }

        /// <summary>
        /// Retorna um usuário pelo número de identificação.
        /// </summary>
        [HttpGet("identification/{identificationNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByIdentificationNumber(string identificationNumber)
        {
            var user = await _userService.GetUserByIdentificationNumberAsync(identificationNumber);
            return user != null ? Ok(user) : NotFound(new { Message = "Usuário não encontrado!" });
        }


        /// <summary>
        /// Altera a senha do usuário.
        /// </summary>
        [HttpPost("change-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            bool success = await _userService.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);

            if (!success)
            {
                return BadRequest(new { Message = "Senha antiga incorreta ou usuário não encontrado!" });
            }

            return Ok(new { Message = "Senha alterada com sucesso!" });
        }
        [HttpGet("{fileName}")]
        [AllowAnonymous]
        public IActionResult GetStaticFile(string fileName)
        {
            // Verifica se o arquivo existe
            var filePath = Path.Combine(_staticFilesPath, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "Arquivo não encontrado." });
            }

            // Obtém o MIME type automaticamente
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream"; // Tipo genérico caso não seja identificado
            }

            // Retorna o arquivo físico como resposta HTTP
            return PhysicalFile(filePath, contentType);
        }

        [HttpPost("upload-image")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Arquivo inválido." });
            }

            try
            {
                // ✅ Nome do arquivo único
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(_uploadsPath, fileName);

                // ✅ Criar a pasta "uploads" caso não exista
                if (!Directory.Exists(_uploadsPath))
                {
                    Directory.CreateDirectory(_uploadsPath);
                }

                // ✅ Salvar o arquivo no servidor
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // ✅ Retornar a URL do arquivo para acesso
                var fileUrl = $"{Request.Scheme}://{Request.Host}/static/uploads/{fileName}";
                return Ok(new { message = "Upload realizado com sucesso!", url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao salvar a imagem.", error = ex.Message });
            }
        }


    }
}

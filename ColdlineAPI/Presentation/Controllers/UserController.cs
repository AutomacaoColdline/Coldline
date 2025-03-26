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
        private readonly string _uploadsPath;

        public UserController(IUserService userService)
        {
            _userService = userService;
            _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
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

        /// <summary>
        /// Atualiza um usuário pelo ID.
        /// </summary>
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            var (updated, message) = await _userService.UpdateUserAsync(id, user);
            
            if (updated)
            {
                return Ok(new { Message = "Usuário atualizado com sucesso!" });
            }
            else
            {
                return BadRequest(new { Message = message });
            }
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
        [AllowAnonymous]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string? name,
            [FromQuery] string? email,
            [FromQuery] string? departmentId,
            [FromQuery] string? userTypeId,
            [FromQuery] int pageNumber = 1,  // Valor padrão
            [FromQuery] int pageSize = 10    // Valor padrão
        )
        {
            // Chamando serviço que faz a busca e devolve (lista + totalCount)
            var (items, totalCount) = await _userService.SearchUsersAsync(
                name,
                email,
                departmentId,
                userTypeId,
                pageNumber,
                pageSize
            );

            // Retornando a lista, a contagem total, e outras informações úteis para o cliente
            // (Ex: quantas páginas existem, página atual, etc.).
            var response = new
            {
                totalCount = totalCount,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                items = items
            };

            return Ok(response);
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

        [HttpGet("uploads/{fileName}")]
        [AllowAnonymous]
        public IActionResult GetStaticFile(string fileName)
        {
            var filePath = Path.Combine(_uploadsPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { Message = "Arquivo não encontrado!" });
            }

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
                // 🔹 Define o diretório de uploads dentro do projeto
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // 🔹 Garante que a pasta existe
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 🔹 Define os caminhos dos arquivos
                var oldFilePath = string.IsNullOrWhiteSpace(oldFileName) ? null : Path.Combine(uploadsFolder, $"{oldFileName}.png");
                var newFilePath = Path.Combine(uploadsFolder, $"{newFileName}.png");

                // ✅ Caso 1: Criando uma nova imagem (nunca existiu antes)
                if (file != null && string.IsNullOrWhiteSpace(oldFileName))
                {
                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return Ok(new { url = $"/uploads/{newFileName}.png" });
                }

                // ✅ Caso 2: Apenas renomeando a imagem (sem upload de novo arquivo)
                if (file == null && !string.IsNullOrWhiteSpace(oldFileName))
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Move(oldFilePath, newFilePath);
                        return Ok(new { url = $"/uploads/{newFileName}.png" });
                    }
                    return NotFound(new { Message = "Imagem anterior não encontrada para renomeação." });
                }

                // ✅ Caso 3: Atualizando a imagem, mas mantendo o nome
                if (file != null && !string.IsNullOrWhiteSpace(oldFileName) && oldFileName == newFileName)
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath); // 🔹 Remove a antiga antes de salvar a nova
                    }

                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok(new { url = $"/uploads/{newFileName}.png" });
                }

                // ✅ Caso 4: Atualizando a imagem e alterando o nome ao mesmo tempo
                if (file != null && !string.IsNullOrWhiteSpace(oldFileName) && oldFileName != newFileName)
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath); // 🔹 Apaga a antiga antes de salvar a nova
                    }

                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok(new { url = $"/uploads/{newFileName}.png" });
                }

                return BadRequest(new { Message = "Nenhuma operação foi realizada. Verifique os parâmetros enviados." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erro ao processar a imagem.", Error = ex.Message });
            }
        }

    }
}

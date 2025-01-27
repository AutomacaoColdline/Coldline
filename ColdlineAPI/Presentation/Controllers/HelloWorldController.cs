using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ColdlineAPI.Application.Interfaces;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloWorldController : ControllerBase
    {
        private readonly IHelloWorldService _helloWorldService;
        private readonly IConfiguration _configuration;

        public HelloWorldController(IHelloWorldService helloWorldService, IConfiguration configuration)
        {
            _helloWorldService = helloWorldService;
            _configuration = configuration;
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult GetPublicMessage()
        {
            // Delegar a lógica para o serviço
            var message = _helloWorldService.GetHelloWorldMessage();
            return Ok(new { Message = $"{message} - Public" });
        }

        [HttpGet("private")]
        [Authorize]
        public IActionResult GetPrivateMessage()
        {
            // Delegar a lógica para o serviço
            var message = _helloWorldService.GetHelloWorldMessage();
            return Ok(new { Message = $"{message} - Protected" });
        }

        [HttpGet("token")]
        [AllowAnonymous]
        public IActionResult GenerateToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"]);

            // Gera o token JWT
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token) });
        }
    }
}

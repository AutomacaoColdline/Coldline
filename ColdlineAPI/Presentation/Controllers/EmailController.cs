using ColdlineAPI.Application.DTOs;
using ColdlineAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailRequest.ToEmail))
                {
                    return BadRequest(new { Message = "O e-mail do destinatário é obrigatório." });
                }

                await _emailService.SendEmailAsync(
                    toEmail: emailRequest.ToEmail,
                    subject: emailRequest.Subject,
                    body: emailRequest.Body
                );

                return Ok(new { Message = "E-mail enviado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Erro ao enviar e-mail: {ex.Message}" });
            }
        }
    }
}

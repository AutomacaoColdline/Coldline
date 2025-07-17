using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArquivoController : ControllerBase
    {
        // ✅ Caminho UNC da rede
        private readonly string _caminhoBase = @"\\10.0.0.201\geral\26. COLDNEX";

        // GET api/arquivo
        [HttpGet]
        public IActionResult ListarArquivos()
        {
            if (!Directory.Exists(_caminhoBase))
                return NotFound("Pasta de rede não encontrada.");

            var arquivos = Directory.GetFiles(_caminhoBase)
                .Select(Path.GetFileName)
                .Select(nome => new
                {
                    Nome = nome,
                    Extensao = Path.GetExtension(nome),
                    CaminhoRede = Path.Combine(_caminhoBase, nome)
                });

            return Ok(arquivos);
        }

        // POST api/arquivo/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadArquivo(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Arquivo inválido.");

            var extensao = Path.GetExtension(arquivo.FileName).ToLower();
            var permitidos = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

            if (!permitidos.Contains(extensao))
                return BadRequest("Tipo de arquivo não permitido.");

            var nomeArquivo = Path.GetFileName(arquivo.FileName);
            var caminhoFinal = Path.Combine(_caminhoBase, nomeArquivo);

            // ✅ Cria a pasta se não existir
            if (!Directory.Exists(_caminhoBase))
                Directory.CreateDirectory(_caminhoBase);

            using (var stream = new FileStream(caminhoFinal, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return Ok(new
            {
                Mensagem = "Arquivo enviado com sucesso.",
                Nome = nomeArquivo,
                CaminhoRede = caminhoFinal
            });
        }
    }
}

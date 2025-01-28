using ColdlineAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ColdlineAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("generate-excel")]
        public IActionResult GenerateExcel([FromBody] List<List<string>> matrix)
        {
            try
            {
                var fileContent = _fileService.CreateExcel(matrix);
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "data.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error generating Excel: {ex.Message}" });
            }
        }

        [HttpPost("generate-pdf")]
        public IActionResult GeneratePdf([FromBody] List<List<string>> matrix)
        {
            try
            {
                var fileContent = _fileService.CreatePdf(matrix);
                return File(fileContent, "application/pdf", "data.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error generating PDF: {ex.Message}" });
            }
        }
    }
}

using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Infrastructure.Utilities;
using System.Collections.Generic;

namespace ColdlineAPI.Application.Services
{
    public class FileService : IFileService
    {
        public byte[] CreateExcel(List<List<string>> matrix)
        {
            return Utils.GenerateExcel(matrix);
        }

        public byte[] CreatePdf(List<List<string>> matrix)
        {
            return Utils.GeneratePdf(matrix);
        }
    }
}

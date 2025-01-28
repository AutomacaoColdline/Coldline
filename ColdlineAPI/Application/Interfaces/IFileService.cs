using System.Collections.Generic;

namespace ColdlineAPI.Application.Interfaces
{
    public interface IFileService
    {
        byte[] CreateExcel(List<List<string>> matrix);
        byte[] CreatePdf(List<List<string>> matrix);
    }
}

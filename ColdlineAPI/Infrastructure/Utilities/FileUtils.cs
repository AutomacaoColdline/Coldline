using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace ColdlineAPI.Infrastructure.Utilities
{
    public static class Utils
    {
        public static byte[] GenerateExcel(List<List<string>> matrix)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            for (int i = 0; i < matrix.Count; i++)
            {
                for (int j = 0; j < matrix[i].Count; j++)
                {
                    worksheet.Cell(i + 1, j + 1).Value = matrix[i][j];
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public static byte[] GeneratePdf(List<List<string>> matrix)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < matrix[0].Count; i++)
                                columns.RelativeColumn();
                        });

                        // Header
                        table.Header(header =>
                        {
                            foreach (var headerText in matrix[0])
                            {
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Text(headerText)
                                    .FontSize(14)
                                    .Bold()
                                    .AlignCenter();
                            }
                        });

                        // Data Rows
                        for (int i = 1; i < matrix.Count; i++)
                        {
                            foreach (var cell in matrix[i])
                            {
                                table.Cell().Text(cell)
                                    .FontSize(12)
                                    .AlignLeft();
                            }
                        }
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
        private static string GenerateNumericCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }
        private static DateTime GetCurrentCampoGrandeTime()
        {
            return TimeZoneInfo.ConvertTime(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande")
            );
        }
    }
}

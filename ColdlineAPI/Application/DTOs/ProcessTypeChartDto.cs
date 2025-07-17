namespace ColdlineAPI.Application.DTOs
{
    public class ProcessTypeChartDto
    {
        public string ProcessTypeName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
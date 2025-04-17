namespace ColdlineAPI.Application.DTOs
{
    public class ProcessStatisticsDto
    {
        public string ProcessTime { get; set; } = "00:00:00"; 
        public string AverageProcessTime { get; set; } = "00:00:00";
        public string StandardDeviation { get; set; } = "00:00:00";
        public string UpperLimit { get; set; } = "00:00:00";
        public string ProcessTypeName { get; set; } = "Não identificado";
        public string OcorrenceTypeName { get; set; } = "Não identificado";
    }
}

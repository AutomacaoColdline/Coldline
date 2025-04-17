namespace ColdlineWeb.Models
{
    public class ProcessStats
    {
        public string ProcessTime { get; set; } = "00:00:00"; 
        public string Avg { get; set; } = "00:00:00";
        public string Std { get; set; } = "00:00:00";
        public string Upper { get; set; } = "00:00:00";
        public string ProcessTypeName { get; set; } = "Não identificado";
        public string OcorrenceTypeName { get; set; } = "Não identificado";
    }
}
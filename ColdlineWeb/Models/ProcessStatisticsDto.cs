
namespace ColdlineWeb.Models
{
    public class ProcessStatisticsDto
    {
        public string ProcessTime  { get; set; } = "00:00:00"; 
        public string AverageProcessTime { get; set; } = "00:00:00";
        public string StandardDeviation { get; set; } = "00:00:00";
        public string UpperLimit { get; set; } = "00:00:00";
    }
}
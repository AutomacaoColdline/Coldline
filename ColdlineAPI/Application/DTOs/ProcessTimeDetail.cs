namespace ColdlineAPI.Application.DTOs
{
    public class ProcessTimeDetail
    {
        public string ProcessId { get; set; } = string.Empty;
        public string ProcessTime { get; set; } = "00:00:00";
        public string ProcessName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
namespace ColdlineAPI.Application.DTOs
{
    public class ProcessTypeTotalTimeDto
    {
        public string ProcessTypeId { get; set; } = string.Empty;
        public string ProcessTypeName { get; set; } = string.Empty;
        public string TotalProcessTime { get; set; } = "00:00:00";
        public int ProcessCount { get; set; }
    }
}

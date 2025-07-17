namespace ColdlineAPI.Application.DTOs
{
    public class MachineTypeAverageTimeDto
    {
        public string MachineTypeId { get; set; } = string.Empty;
        public string MachineTypeName { get; set; } = string.Empty;
        public string AverageProcessTime { get; set; } = "00:00:00";
        public int MachineCount { get; set; }
    }
}
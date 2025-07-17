namespace ColdlineAPI.Application.DTOs
{
    public class MachineTotalProcessTimeDto
    {
        public string MachineId { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string TotalProcessTime { get; set; } = "00:00:00";
        public int ProcessCount { get; set; }
        public string MachineTypeName { get; set; } = "Desconhecido";
    }

}
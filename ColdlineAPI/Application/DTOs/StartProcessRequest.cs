namespace ColdlineAPI.Application.DTOs
{
    public class StartProcessRequest
    {
        public string IdentificationNumber { get; set; } = string.Empty;
        public string ProcessTypeId { get; set; } = string.Empty;
        public string? MachineId { get; set; } 
        public bool  PreIndustrialization { get; set; }
        public bool ReWork { get; set; }
        public bool  Prototype { get; set; }
    }
}
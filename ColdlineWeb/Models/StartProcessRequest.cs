namespace ColdlineWeb.Models
{
    public class StartProcessRequest
    {
        public string ProcessTypeId { get; set; } = string.Empty;
        public string? MachineId { get; set; } = string.Empty;
        public bool PreIndustrialization { get; set; }
        public bool ReWork { get; set; }
    }
}

namespace ColdlineWeb.Models
{
    public class StartProcessModel
    {
        public string ProcessTypeId { get; set; } = string.Empty;
        public string? MachineId { get; set; } = string.Empty;
        public bool PreIndustrialization { get; set; }
        public bool ReWork { get; set; }
        public bool Prototype { get; set; }
    }
}

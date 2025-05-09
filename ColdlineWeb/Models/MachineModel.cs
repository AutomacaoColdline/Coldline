using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Models{
    public class MachineModel
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string Voltage { get; set; } = string.Empty;
        public ReferenceEntity? Process { get; set; } 
        public ReferenceEntity? Quality { get; set; } 
        public ReferenceEntity? Monitoring { get; set; } 
        public ReferenceEntity MachineType { get; set; } = new();
        public MachineStatus? Status { get; set; }
        public string Time { get; set; } = "00:00:00";
    }

}

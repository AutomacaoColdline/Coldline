namespace ColdlineWeb.Models{
    public class MachineModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string Voltage { get; set; } = string.Empty;
        public ProcessModel Process { get; set; } = new();
        public ReferenceEntity Quality { get; set; } = new();
        public ReferenceEntity Monitoring { get; set; } = new(); // Ajustado para pegar diretamente o Name
    }

}

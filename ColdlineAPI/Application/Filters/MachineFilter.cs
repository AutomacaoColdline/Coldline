using System;

namespace ColdlineAPI.Application.Filters
{
    public class MachineFilter
    {
        public string? Name { get; set; }
        public string? CustomerName { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? Phase { get; set; }
        public string? Voltage { get; set; }
        public string? ProcessId { get; set; }
        public string? QualityId { get; set; }
        public string? MonitoringId { get; set; }
    }
}

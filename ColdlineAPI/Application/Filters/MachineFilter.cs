using System;
using ColdlineAPI.Domain.Enum;

namespace ColdlineAPI.Application.Filters
{
    public class MachineFilter
    {
        public string? CustomerName { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? Phase { get; set; }
        public string? Voltage { get; set; }
        public string? ProcessId { get; set; }
        public string? QualityId { get; set; }
        public string? MachineTypeId { get; set; }
        public string? MonitoringId { get; set; }
        public MachineStatus? Status { get; set; }
    }
}

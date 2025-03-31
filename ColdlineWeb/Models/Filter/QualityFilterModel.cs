using ColdlineWeb.Models.Enum;

namespace ColdlineWeb.Models.Filter
{
    public class QualityFilterModel
    {
        public string? TotalPartValue { get; set; }
        public string? WorkHourCost { get; set; }
        public string? DepartamentId { get; set; }
        public string? MachineId { get; set; }
        public List<string>? OccurrencesIds { get; set; }
        
    }
}

using System.Collections.Generic;

namespace ColdlineAPI.Application.Filters
{
    public class QualityFilter
    {
        public string? TotalPartValue { get; set; }
        public string? WorkHourCost { get; set; }
        public string? DepartamentId { get; set; }
        public string? MachineId { get; set; }
        public List<string>? OccurrencesIds { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? SortBy { get; set; }      
        public bool? SortDesc { get; set; }
    }
}
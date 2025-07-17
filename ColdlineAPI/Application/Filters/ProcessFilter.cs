using System;
using System.Collections.Generic;

namespace ColdlineAPI.Application.Filters
{
    public class ProcessFilter
    {
        public string? IdentificationNumber { get; set; }
        public string? ProcessTime { get; set; } 
        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; } 
        public string? UserId { get; set; }
        public string? DepartmentId { get; set; }
        public string? ProcessTypeId { get; set; }
        public string? PauseTypesId { get; set; }
        public List<string>? OccurrencesIds { get; set; }
        public string? MachineId { get; set; }
        public bool? Finished { get; set; }
        public bool? PreIndustrialization { get; set; }
        public bool? Prototype {get; set;}
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}

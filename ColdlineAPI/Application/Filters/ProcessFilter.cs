using System;
using System.Collections.Generic;

namespace ColdlineAPI.Application.Filters
{
    public class ProcessFilter
    {
        public string? IdentificationNumber { get; set; }
        public string? ProcessTime { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? UserId { get; set; }
        public string? DepartamentId { get; set; }
        public string? ProcessTypeId { get; set; }
        public string? PauseTypesId { get; set; }
        public List<string>? OccurrencesIds { get; set; }
        public string? MachineId { get; set; }
    }
}

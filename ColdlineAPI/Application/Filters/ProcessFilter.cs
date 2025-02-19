using System;
using System.Collections.Generic;

namespace ColdlineAPI.Application.Filters
{
    public class ProcessFilter
    {
        public string? IdentificationNumber { get; set; }
        public string? ProcessTime { get; set; } // Mantém como string (formato HH:mm:ss)
        public DateTime? StartDate { get; set; } // Agora como DateTime para comparação correta
        public DateTime? EndDate { get; set; } // Agora como DateTime para comparação correta
        public string? UserId { get; set; }
        public string? DepartmentId { get; set; }
        public string? ProcessTypeId { get; set; }
        public string? PauseTypesId { get; set; }
        public List<string>? OccurrencesIds { get; set; }
        public string? MachineId { get; set; }
    }
}

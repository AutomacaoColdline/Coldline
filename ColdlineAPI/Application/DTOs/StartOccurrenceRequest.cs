using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Application.DTOs
{
    public class StartOccurrenceRequest
    {
        public string CodeOccurrence { get; set; } = string.Empty; // Obrigatório
        public ReferenceEntity PauseType { get; set; } = new ReferenceEntity(); // Obrigatório
        public ReferenceEntity? Process { get; set; } = null;
        public ReferenceEntity? Defect { get; set; } = null;
        public ReferenceEntity User { get; set; } = new ReferenceEntity();
    }
}

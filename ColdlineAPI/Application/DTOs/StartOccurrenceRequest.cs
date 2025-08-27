using ColdlineAPI.Domain.Common;
using ColdlineAPI.Domain.Entities;
namespace ColdlineAPI.Application.DTOs
{
    public class StartOccurrenceRequest
    {
        public string? CodeOccurrence { get; set; } = string.Empty; // Obrigatório
        public OccurrenceType OccurrenceType { get; set; } = new OccurrenceType();
        public ReferenceEntity? Process { get; set; } = null;
        public ReferenceEntity User { get; set; } = new ReferenceEntity();
        public bool MachineStopped { get; set; }
        public string Description { get; set; } = string.Empty;
        public ReferenceEntity? Part { get; set; } = new ReferenceEntity();
    }
}

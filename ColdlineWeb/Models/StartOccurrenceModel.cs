using System;
using ColdlineWeb.Models;

namespace ColdlineWeb.Models
{
    public class StartOccurrenceModel
    {
        public string CodeOccurrence { get; set; } = string.Empty; // Obrigatório
        public OccurrenceTypeModel OccurrenceType { get; set; } = new OccurrenceTypeModel();
        public ReferenceEntity? Process { get; set; } = null;
        public ReferenceEntity User { get; set; } = new ReferenceEntity();
        public ReferenceEntity? Department { get; set; } = new();
        public bool MachineStopped { get; set; }
        public string Description { get; set; } = string.Empty;
        public ReferenceEntity? Part { get; set; } = new ReferenceEntity();
    }
}

using System;
using ColdlineWeb.Models;

namespace ColdlineWeb.Models
{
    public class StartOccurrenceModel
    {
        public string CodeOccurrence { get; set; } = string.Empty; // Obrigatório
        public ReferenceEntity PauseType { get; set; } = new ReferenceEntity(); // Obrigatório
        public ReferenceEntity Process { get; set; } = new ReferenceEntity(); // Obrigatório (diferente da API)
        public ReferenceEntity? Defect { get; set; } = null;
        public ReferenceEntity User { get; set; } = new ReferenceEntity();
    }
}

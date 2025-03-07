namespace ColdlineWeb.Models
{
    public class OccurrenceModel
    {
        public string Id { get; set; } = string.Empty;
        public string CodeOccurrence { get; set; } = string.Empty;
        public string ProcessTime { get; set; } = "00:00:00";
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public ReferenceEntity Process { get; set; } = new();
        public ReferenceEntity PauseType { get; set; } = new();
        public ReferenceEntity? Defect { get; set; }
        public ReferenceEntity User { get; set; } = new();
        public bool Finished { get; set; }
    }
}

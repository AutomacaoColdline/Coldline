namespace ColdlineWeb.Models{
    public class ProcessModel
    {
        public string Id { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string ProcessTime { get; set; } = "00:00:00";
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public ReferenceEntity User { get; set; } = new();
        public ReferenceEntity Department { get; set; } = new();
        public ReferenceEntity ProcessType { get; set; } = new();
        public ReferenceEntity Machine { get; set; } = new();
        public List<ReferenceEntity>? Occurrences { get; set; } = new();
        public bool InOccurrence { get; set; }
        public bool? PreIndustrialization { get; set; }
        public bool Finished { get; set; }
        public bool ReWork { get; set; }
    }


}


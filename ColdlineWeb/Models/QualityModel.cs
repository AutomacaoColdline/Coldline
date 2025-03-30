namespace ColdlineWeb.Models{
    public class QualityModel
    {
        public string Id { get; set; } = string.Empty;
        public string TotalPartValue { get; set; } = string.Empty;

        public string WorkHourCost { get; set; } = string.Empty;
        public List<ReferenceEntity>? Occurrences { get; set; } = new List<ReferenceEntity>();
        public ReferenceEntity? Departament { get; set; } = new ReferenceEntity();
        public ReferenceEntity? Machine { get; set; } = new ReferenceEntity();
    }

}

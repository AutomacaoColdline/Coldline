namespace ColdlineWeb.Models{
    public class DefectModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Internal { get; set; } 
        public ReferenceEntity TypeDefect { get; set; } = new ReferenceEntity();
        public ReferenceEntity Part { get; set; } = new ReferenceEntity();
    }

}

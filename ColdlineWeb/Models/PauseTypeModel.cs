namespace ColdlineWeb.Models{
    public class PauseTypeModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReferenceEntity? Defect { get; set; } = new ReferenceEntity();
        public bool? Rework { get; set; }
    }

}

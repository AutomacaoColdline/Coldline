namespace ColdlineWeb.Models{
    public class UserTypeModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReferenceEntity? Department { get; set; } = new();
    }
}

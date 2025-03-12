namespace ColdlineWeb.Models{
    public class UserModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ReferenceEntity UserType { get; set; } = new();
        public ReferenceEntity Department { get; set; } = new();
        public ReferenceEntity? CurrentProcess { get; set; }
        public ReferenceEntity? CurrentOccurrence { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string UrlPhoto { get; set; } = string.Empty;
    }


}


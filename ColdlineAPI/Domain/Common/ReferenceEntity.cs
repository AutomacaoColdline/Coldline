namespace ColdlineAPI.Domain.Common
{
    public class ReferenceEntity
    {
        public string Id { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty;
        
         public ReferenceEntity() { }

        public ReferenceEntity(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

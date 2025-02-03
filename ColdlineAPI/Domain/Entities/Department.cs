using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ColdlineAPI.Domain.Entities
{
    public class Department
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }  // Agora o MongoDB gerará o ID automaticamente.

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
    }
}

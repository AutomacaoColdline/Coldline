using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ColdlineAPI.Domain.Entities
{
    public class Part
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }  // Permite ser nulo para ser gerado automaticamente

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("value")]
        public double Value { get; set; }
    }
}

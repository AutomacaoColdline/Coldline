using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ColdlineAPI.Domain.Entities
{
    public class MachineType
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

    }
}
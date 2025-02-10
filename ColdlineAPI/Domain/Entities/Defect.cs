using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class Defect
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        [BsonElement("internal")]
        public bool Internal { get; set; } 
        [BsonElement("type defect")]
        public ReferenceEntity TypeDefect { get; set; } = new ReferenceEntity();
        [BsonElement("part")]
        public ReferenceEntity Part { get; set; } = new ReferenceEntity();
    }
}

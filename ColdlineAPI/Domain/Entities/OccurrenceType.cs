using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class OccurrenceType
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        [BsonElement("Pending event")]
        public bool PendingEvent { get; set; }
        [BsonElement("departament")]
        public ReferenceEntity? Department { get; set; } = new ReferenceEntity();
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class Occurrence
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 
        [BsonElement("Process")]
        public ReferenceEntity Process { get; set; } = new ReferenceEntity();

        [BsonElement("Pause Type")]
        public ReferenceEntity PauseType { get; set; } = new ReferenceEntity();
        [BsonElement("Defect")]
        public ReferenceEntity Defect { get; set; } = new ReferenceEntity();
        [BsonElement("User")]
        public ReferenceEntity User { get; set; } = new ReferenceEntity();
    }
}

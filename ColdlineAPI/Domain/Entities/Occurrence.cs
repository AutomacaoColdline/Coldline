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
        [BsonElement("process")]
        public ReferenceEntity Process { get; set; } = new ReferenceEntity();

        [BsonElement("pause Type")]
        public ReferenceEntity PauseType { get; set; } = new ReferenceEntity();
        [BsonElement("defect")]
        public ReferenceEntity Defect { get; set; } = new ReferenceEntity();
        [BsonElement("user")]
        public ReferenceEntity User { get; set; } = new ReferenceEntity();
    }
}

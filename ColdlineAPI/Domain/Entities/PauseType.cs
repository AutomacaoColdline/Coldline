using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class PauseType
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }  // Permite ser nulo para ser gerado automaticamente

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        [BsonElement("start date")]
        public string StartDate { get; set; } = string.Empty;
        [BsonElement("end date")]
        public string EndDate { get; set; } = string.Empty;
        [BsonElement("Defect")]
        public ReferenceEntity Defect { get; set; } = new ReferenceEntity();
    }
}

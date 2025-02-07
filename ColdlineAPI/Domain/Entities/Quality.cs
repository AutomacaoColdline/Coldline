using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class Quality
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("total part value")]
        public string TotalPartValue { get; set; } = string.Empty;

        [BsonElement("work hour cost")]
        public string WorkHourCost { get; set; } = string.Empty;
        [BsonElement("ocurrences")]
        public List<ReferenceEntity> Occurrences { get; set; } = new List<ReferenceEntity>();
        [BsonElement("departament")]
        public ReferenceEntity Departament { get; set; } = new ReferenceEntity();
        [BsonElement("machine")]
        public ReferenceEntity Machine { get; set; } = new ReferenceEntity();
    }
}

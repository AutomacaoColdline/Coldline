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

        [BsonElement("codeOccurence")]
        public string CodeOccurrence { get; set; } = string.Empty;

        [BsonElement("process time")]
        public string ProcessTime { get; set; } = "00:00:00"; // Agora armazenado como string

        [BsonElement("start date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [BsonElement("end date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; } = DateTime.UtcNow;

        [BsonElement("process")]
        public ReferenceEntity? Process { get; set; } = new ReferenceEntity();

        [BsonElement("user")]
        public ReferenceEntity User { get; set; } = new ReferenceEntity();

        [BsonElement("departament")]
        public ReferenceEntity? Department { get; set; } = new ReferenceEntity();
        [BsonElement("finished")]
        public bool? Finished { get; set; }
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        [BsonElement("occurrence Type")]
        public OccurrenceType? OccurrenceType { get; set; } = new OccurrenceType();
        [BsonElement("part")]
        public ReferenceEntity? Part { get; set; } = new ReferenceEntity();
        [BsonElement("machine")]
        public ReferenceEntity? Machine { get; set; } = new ReferenceEntity();
    }
}

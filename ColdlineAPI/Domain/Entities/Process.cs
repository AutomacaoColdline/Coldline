using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class Process
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        [BsonElement("identification number")]
        public string IdentificationNumber { get; set; } = string.Empty;

        [BsonElement("process time")]
        public string ProcessTime { get; set; } = "00:00:00"; // Agora armazenado como string

        [BsonElement("start date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [BsonElement("end date")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; } = DateTime.UtcNow;

        [BsonElement("user")]
        public ReferenceEntity? User { get; set; } = new ReferenceEntity();

        [BsonElement("departament")]
        public ReferenceEntity? Department { get; set; } = new ReferenceEntity();

        [BsonElement("process type")]
        public ReferenceEntity? ProcessType { get; set; } = new ReferenceEntity();

        [BsonElement("pause types")]
        public ReferenceEntity? PauseTypes { get; set; } = new ReferenceEntity();

        [BsonElement("occurrences")]
        public List<ReferenceEntity>? Occurrences { get; set; } = new List<ReferenceEntity>();

        [BsonElement("machine")]
        public ReferenceEntity? Machine { get; set; } = new ReferenceEntity();
        [BsonElement("InOccurrence")]
        public bool? InOccurrence { get; set; }
        [BsonElement("finished")]
        public bool? Finished { get; set; }
        [BsonElement("preIndustrialization")]
        public bool? PreIndustrialization { get; set; }
        [BsonElement("ReWork")]
        public bool? ReWork { get; set; }
    }
}

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
        public string ProcessTime { get; set; } = string.Empty;
        [BsonElement("start date")]
        public string StartDate { get; set; } = string.Empty;
        [BsonElement("end date")]
        public string EndDate { get; set; } = string.Empty;
        [BsonElement("user")]
        public ReferenceEntity User { get; set; } = new ReferenceEntity();
        [BsonElement("departament")]
        public ReferenceEntity Departament { get; set; } = new ReferenceEntity();
        [BsonElement("process type")]
        public ReferenceEntity ProcessType { get; set; } = new ReferenceEntity();
        [BsonElement("pause types")]
        public ReferenceEntity PauseTypes { get; set; } = new ReferenceEntity();
        [BsonElement("occurrences")]
        public List<ReferenceEntity> Occurrences { get; set; } = new List<ReferenceEntity>();
        [BsonElement("machine")]
        public ReferenceEntity Machine { get; set; } = new ReferenceEntity();
    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class Machine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("customer name")]
        public string CustomerName { get; set; } = string.Empty;
        [BsonElement("identification number")]
        public string IdentificationNumber { get; set; } = string.Empty;
        [BsonElement("phase")]
        public string Phase { get; set; } = string.Empty;
        [BsonElement("voltage")]
        public string Voltage { get; set; } = string.Empty;
        [BsonElement("process")]
        public ReferenceEntity? Process { get; set; } = new ReferenceEntity();
        [BsonElement("quality")]
        public ReferenceEntity? Quality { get; set; } = new ReferenceEntity();
        [BsonElement("monitoring")]
        public ReferenceEntity? Monitoring { get; set; } = new ReferenceEntity();
        [BsonElement("finished")]
        public bool? Finished { get; set; }
    }
}
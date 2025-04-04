using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;
using ColdlineAPI.Domain.Enum;

namespace ColdlineAPI.Domain.Entities
{
    public class Machine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

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
        [BsonElement("status")]
        public MachineStatus Status { get; set; }
        [BsonElement("machineType")]
        public ReferenceEntity? MachineType { get; set; } = new ReferenceEntity();
        [BsonElement("time")]
        public string Time { get; set; } = "00:00:00";
    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class Monitoring
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }  

        [BsonElement("gateway")]
        public string? Gateway { get; set; } = string.Empty;
        [BsonElement("ihm")]
        public string? IHM { get; set; } = string.Empty;
        [BsonElement("clp")]
        public List<string>? CLP { get; set; } = new List<string>();

        [BsonElement("id rustdesk")]
        public string? IdRustdesk { get; set; } = string.Empty;
        [BsonElement("id anydesk")]
        public string? IdAnydesk { get; set; } = string.Empty;
        [BsonElement("id team viewer")]
        public string? IdTeamViewer { get; set; } = string.Empty;
        [BsonElement("Monitoring Type")]
        public ReferenceEntity? MonitoringType { get; set; } = new ReferenceEntity();
    }
}

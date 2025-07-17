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

        [BsonElement("identificador")]
        public string Identificador { get; set; } = string.Empty;

        [BsonElement("unidade")]
        public string Unidade { get; set; } = string.Empty;

        [BsonElement("estado")]
        public string Estado { get; set; } = string.Empty;

        [BsonElement("cidade")]
        public string Cidade { get; set; } = string.Empty;

        [BsonElement("ihm")]
        public string IHM { get; set; } = string.Empty;

        [BsonElement("gateway")]
        public string Gateway { get; set; } = string.Empty;

        [BsonElement("clp")]
        public List<string> CLP { get; set; } = new List<string>();

        [BsonElement("macs")]
        public List<string> MACs { get; set; } = new List<string>();

        [BsonElement("mascara")]
        public string MASC { get; set; } = string.Empty;

        [BsonElement("id anydesk")]
        public string IdAnydesk { get; set; } = string.Empty;

        [BsonElement("id rustdesk")]
        public string IdRustdesk { get; set; } = string.Empty;

        [BsonElement("id teamviewer")]
        public string IdTeamViewer { get; set; } = string.Empty;

        [BsonElement("monitoring_type")]
        public ReferenceEntity MonitoringType { get; set; } = new ReferenceEntity();
    }
}

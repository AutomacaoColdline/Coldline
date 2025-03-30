namespace ColdlineWeb.Models{
    public class MonitoringModel
    {
        public string Id { get; set; } = string.Empty;
        using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ColdlineAPI.Domain.Common;

namespace ColdlineAPI.Domain.Entities
{
    public class Monitoring
    {
        public string? Id { get; set; }  

        public string? Gateway { get; set; } = string.Empty;
        public string? IHM { get; set; } = string.Empty;
        public List<string>? CLP { get; set; } = new List<string>();

        public string? IdRustdesk { get; set; } = string.Empty;
        public string? IdAnydesk { get; set; } = string.Empty;
        public string? IdTeamViewer { get; set; } = string.Empty;
        public ReferenceEntity? MonitoringType { get; set; } = new ReferenceEntity();
    }
}

    }

}

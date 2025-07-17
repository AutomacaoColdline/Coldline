namespace ColdlineWeb.Models{
    public class MonitoringModel
    {
        public string Id { get; set; } = string.Empty;
        public string Identificador { get; set; } = string.Empty;
        public string Unidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Ihm { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public List<string> Clp { get; set; } = new();
        public List<string> Macs { get; set; } = new();
        public string Masc { get; set; } = string.Empty;
        public string IdAnydesk { get; set; } = string.Empty;
        public string IdRustdesk { get; set; } = string.Empty;
        public string IdTeamViewer { get; set; } = string.Empty;
        public MonitoringTypeModel MonitoringType { get; set; } = new();
    }
}

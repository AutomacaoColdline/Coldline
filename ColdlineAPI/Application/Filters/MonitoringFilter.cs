namespace ColdlineAPI.Application.Filters
{
    public class MonitoringFilter
    {
        public string? Estado { get; set; }
        public string? Cidade { get; set; }
        public string? Identificador { get; set; }
        public string? Unidade { get; set; }
        public string? MonitoringTypeId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

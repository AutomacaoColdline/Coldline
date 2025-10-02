namespace ColdlineAPI.Application.Filters
{
    public class MonitoringFilter
    {
        public string? Estado { get; set; }
        public string? Cidade { get; set; }
        public string? Identificador { get; set; }
        public string? Unidade { get; set; }
        public string? MonitoringTypeId { get; set; }

        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? SortBy { get; set; }      
        public bool? SortDesc { get; set; }
    }
}

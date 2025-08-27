namespace ColdlineAPI.Application.Filters
{
    public class OccurrenceSearchFilter
    {
        public string? UserId { get; set; }
        public bool? Finished { get; set; }
        public string? OccurrenceTypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }    
        public string? MachineID  { get; set; }      
    }
}

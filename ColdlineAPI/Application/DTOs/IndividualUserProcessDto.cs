namespace ColdlineAPI.Application.DTOs
{
    public class IndividualUserProcessDto
    {
        public string ProcessId { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string ProcessTypeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProcessTime { get; set; } = "00:00:00";
    }
}

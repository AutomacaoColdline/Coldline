namespace ColdlineAPI.Application.DTOs
{
    public class StartProcessRequest
    {
        public string IdentificationNumber { get; set; } = string.Empty;
        public string ProcessTypeId { get; set; } = string.Empty;
    }
}
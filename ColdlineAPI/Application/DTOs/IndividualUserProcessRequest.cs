namespace ColdlineAPI.Application.DTOs
{
    public class IndividualUserProcessRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool? PreIndustrialization { get; set; } // Novo campo opcional
    }
}

namespace ColdlineAPI.Application.DTOs
{
    public class UserProcessDetailsDto
    {
        // Informações básicas do usuário
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UrlPhoto { get; set; } = string.Empty;
        public string TypeProcessName { get; set; } = string.Empty;

        // Informações do processo atual
        public string CurrentProcessId { get; set; } = string.Empty;
        public string ProcessTime { get; set; } = "00:00:00";
        public string AverageProcessTime { get; set; } = "00:00:00";
        public string StandardDeviation { get; set; } = "00:00:00";
        public string UpperLimit { get; set; } = "00:00:00";

        // Ocorrência
        public string CurrentOccurrenceName { get; set; } = "Nenhuma";
    }
}

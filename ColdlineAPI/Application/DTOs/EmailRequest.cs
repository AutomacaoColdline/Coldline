namespace ColdlineAPI.Application.DTOs
{
    public class EmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = "Sem Assunto"; // Valor padrão
        public string Body { get; set; } = string.Empty;
    }
}

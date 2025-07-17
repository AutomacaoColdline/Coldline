namespace ColdlineAPI.Application.DTOs
{
    public class UserTotalProcessTimeDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string TotalProcessTime { get; set; } = "00:00:00";
        public int ProcessCount { get; set; } // ✅ Novo campo
    }
}
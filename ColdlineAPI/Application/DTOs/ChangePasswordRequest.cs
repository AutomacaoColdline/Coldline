namespace ColdlineAPI.Application.DTOs
{
    public class ChangePasswordRequest
    {
        public string UserId { get; set; } = string.Empty; 
        public string OldPassword { get; set; } = string.Empty; 
        public string NewPassword { get; set; } = string.Empty;
    }
}

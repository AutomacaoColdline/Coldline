namespace ColdlineAPI.Infrastructure.Configurations
{
    public class SmtpConfig
    {
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

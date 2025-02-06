using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ColdlineAPI.Infrastructure.Configurations
{
    public static class DatabaseConfig
    {
        public static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            // Configuração do MongoDB via DI
            services.Configure<MongoDBSettings>(configuration.GetSection("MongoDB"));
            services.Configure<SmtpConfig>(configuration.GetSection("SMTP"));
        }
    }
}

// using necessários
using MongoDB.Driver;
using ColdlineAPI.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ColdlineAPI.Infrastructure.Configurations
{
    public static class DatabaseConfig
    {
        public static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDBSettings>(configuration.GetSection("MongoDB"));

            // ✅ Adiciona o MongoClient como Singleton (reutilizável, thread-safe)
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });
            services.Configure<SmtpConfig>(configuration.GetSection("SMTP"));
        }
    }
}

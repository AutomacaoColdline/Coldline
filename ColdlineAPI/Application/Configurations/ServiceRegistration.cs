using Microsoft.Extensions.DependencyInjection;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Services;

namespace ColdlineAPI.Application.Configurations
{
    public static class ServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IHelloWorldService, HelloWorldService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUserTypeService, UserTypeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ITypeDefectService, TypeDefectService>();
            services.AddScoped<IPartService, PartService>();
            services.AddScoped<IDefectService, DefectService>();
            services.AddScoped<IPauseTypeService, PauseTypeService>();
            services.AddScoped<IOccurrenceService, OccurrenceService>();
            services.AddScoped<IMonitoringTypeService, MonitoringTypeService>();
        }
    }
}

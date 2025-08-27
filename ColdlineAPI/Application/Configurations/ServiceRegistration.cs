using Microsoft.Extensions.DependencyInjection;
using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Services;
using ColdlineAPI.Application.Factories;

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
            services.AddScoped<IMonitoringService, MonitoringService>();
            services.AddScoped<IQualityService, QualityService>();
            services.AddScoped<IProcessService, ProcessService>();
            services.AddScoped<IProcessTypeService, ProcessTypeService>();
            services.AddScoped<IMachineService, MachineService>();
            services.AddScoped<IMachineTypeService, MachineTypeService>();
            services.AddScoped<IOccurrenceTypeService, OccurrenceTypeService>();
            services.AddScoped<RepositoryFactory>();


        }
    }
}

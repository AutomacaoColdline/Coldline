using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ColdlineWeb;
using ColdlineWeb.Services;

// Blazorise
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Componentes principais
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://10.0.0.44:4000/")
});

// Serviços
builder.Services.AddScoped<IndustriaService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MachineService>();
builder.Services.AddScoped<ProcessService>();
builder.Services.AddScoped<DefectService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<UserTypeService>();
builder.Services.AddScoped<PauseTypeService>();
builder.Services.AddScoped<PartService>();
builder.Services.AddScoped<MachineTypeService>();
builder.Services.AddScoped<ProcessTypeService>();
builder.Services.AddScoped<QualityService>();
builder.Services.AddScoped<TypeDefectService>();
builder.Services.AddScoped<OccurrenceService>();
builder.Services.AddScoped<OccurrenceTypeService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<MonitoringService>();
builder.Services.AddScoped<MonitoringTypeService>();

// Inicializa Blazorise
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

// Executa o app
await builder.Build().RunAsync();

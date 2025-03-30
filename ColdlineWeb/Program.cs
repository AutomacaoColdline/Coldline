using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ColdlineWeb;
using ColdlineWeb.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://10.0.0.44:5000/")
});

builder.Services.AddScoped<IndustriaService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MachineService>();
builder.Services.AddScoped<ProcessService>();
builder.Services.AddScoped<DefectService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<UserTypeService>();
builder.Services.AddScoped<PauseTypeService>();
builder.Services.AddScoped<ProcessTypeService>();
builder.Services.AddScoped<QualityService>();
builder.Services.AddScoped<TypeDefectService>();


await builder.Build().RunAsync();

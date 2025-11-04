using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ColdlineWeb;
using ColdlineWeb.Services;
using ColdlineWeb.Models.Config;
using System.Net.Http;

// ==============================
// Configuração principal
// ==============================
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ------------------------------
// Carrega appsettings.json do wwwroot
// ------------------------------
try
{
    using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    using var response = await http.GetAsync("appsettings.json"); // <- 404 estava aqui
    response.EnsureSuccessStatusCode();
    using var stream = await response.Content.ReadAsStreamAsync();
    builder.Configuration.AddJsonStream(stream);
}
catch (Exception ex)
{
    // Loga em console para facilitar diagnósticos e segue com defaults
    Console.Error.WriteLine($"[ColdlineWeb] Falha ao carregar appsettings.json: {ex.Message}");
}

// ------------------------------
// Lê configuração da API (com fallback claro)
// ------------------------------
var apiBaseUrl = builder.Configuration.GetValue<string>("ServerSettings:ApiBaseUrl");
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    // Fallback pra não travar; ajuste se preferir
    apiBaseUrl = "http://localhost:4000";
    Console.Error.WriteLine($"[ColdlineWeb] 'ServerSettings:ApiBaseUrl' não definido. Usando fallback: {apiBaseUrl}");
}

// ------------------------------
// Componentes raiz
// ------------------------------
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ------------------------------
// HttpClient global
// ------------------------------
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// ------------------------------
// Options<ServerSettings> (requer Microsoft.Extensions.Options.DataAnnotations)
// ------------------------------
builder.Services
    .AddOptions<ServerSettings>()
    .Bind(builder.Configuration.GetSection("ServerSettings"))
    .ValidateDataAnnotations();

// ------------------------------
// Serviços
// ------------------------------
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

await builder.Build().RunAsync();

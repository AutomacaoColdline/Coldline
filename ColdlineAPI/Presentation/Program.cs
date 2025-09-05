using ColdlineAPI.Application.Configurations;
using ColdlineAPI.Infrastructure.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using System.IO;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// 1) CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://10.0.0.44:5173",
                "http://coldline.industria.com",
                "http://coldline.coldnex.com",
                "http://coldnex.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 2) Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

ServiceRegistration.ConfigureServices(builder.Services);
AuthenticationConfig.ConfigureAuthentication(builder.Services, builder.Configuration);
DatabaseConfig.ConfigureDatabase(builder.Services, builder.Configuration);
SwaggerConfig.ConfigureSwagger(builder.Services); // registra Swagger sempre

var app = builder.Build();

// 3) CORS primeiro
app.UseCors("AllowAll");

// 4) Swagger em Dev OU quando habilitado no appsettings.{Environment}.json
var enableSwaggerInProd = builder.Configuration.GetValue<bool>("Swagger:EnableInProduction", false);
if (app.Environment.IsDevelopment() || enableSwaggerInProd)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ColdlineBackEnd API v1");
        c.RoutePrefix = "swagger";
    });

    // redireciona raiz para /swagger (opcional)
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

// 5) Arquivos estáticos (uploads)
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// 6) Pipeline
// Em contêiner sem HTTPS publicado, evite redirecionar para https em produção.
// Mantemos apenas no Development para não gerar warning de porta HTTPS.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

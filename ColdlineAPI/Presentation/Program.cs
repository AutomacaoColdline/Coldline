using ColdlineAPI.Application.Configurations;
using ColdlineAPI.Infrastructure.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Lê as origens permitidas do appsettings
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

// ✅ Configura o CORS dinâmico
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// 🧱 Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

ServiceRegistration.ConfigureServices(builder.Services);
AuthenticationConfig.ConfigureAuthentication(builder.Services, builder.Configuration);
DatabaseConfig.ConfigureDatabase(builder.Services, builder.Configuration);
SwaggerConfig.ConfigureSwagger(builder.Services); // sempre registra

var app = builder.Build();

// ⚙️ Middleware CORS primeiro
app.UseCors("DevCors");

// 🌱 Facilidades de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ColdlineBackEnd API v1");
        c.RoutePrefix = "swagger";
    });

    // opcional: abrir o swagger na raiz
    app.MapGet("/", () => Results.Redirect("/swagger"));

    // Habilite se estiver usando dev-certs e HTTPS local:
    // app.UseHttpsRedirection();
}

// 📂 Static files (uploads)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

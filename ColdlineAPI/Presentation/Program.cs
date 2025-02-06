using ColdlineAPI.Application.Configurations; // Agora chamamos o ServiceRegistration corretamente
using ColdlineAPI.Infrastructure.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configuração dos serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Ajustado para chamar ServiceRegistration do Application
ServiceRegistration.ConfigureServices(builder.Services);
AuthenticationConfig.ConfigureAuthentication(builder.Services, builder.Configuration);
DatabaseConfig.ConfigureDatabase(builder.Services, builder.Configuration);
SwaggerConfig.ConfigureSwagger(builder.Services);

var app = builder.Build();

// ✅ Aplicar CORS antes dos middlewares
app.UseCors("AllowAll");

// ✅ Configuração do Swagger para ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ColdlineBackEnd API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

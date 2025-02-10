using ColdlineAPI.Application.Configurations;
using ColdlineAPI.Infrastructure.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. CONFIGURAR CORS PERMITINDO O BLAZOR WEBASSEMBLY
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://10.0.0.44:5173", "http://coldline.industria.com") // 🔹 Incluímos a origem do Blazor
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});


// ✅ 2. Configuração dos serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
ServiceRegistration.ConfigureServices(builder.Services);
AuthenticationConfig.ConfigureAuthentication(builder.Services, builder.Configuration);
DatabaseConfig.ConfigureDatabase(builder.Services, builder.Configuration);
SwaggerConfig.ConfigureSwagger(builder.Services);

var app = builder.Build();

// ✅ 3. **APLICAR O CORS ANTES DOS OUTROS MIDDLEWARES**
app.UseCors("AllowAll");

// ✅ 4. Configuração do Swagger para ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ColdlineBackEnd API v1");
    });
}

// ✅ 5. Ordem correta dos middlewares
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

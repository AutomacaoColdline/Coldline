using ColdlineAPI.Application.Interfaces;
using ColdlineAPI.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar o host para escutar em todas as interfaces
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Configurar serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ColdlineBackEnd API", Version = "v1" });

    // Configurar o esquema de autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar autenticação JWT
var key = Encoding.ASCII.GetBytes(builder.Configuration["JWT:SecretKey"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Registrar serviços de aplicação
builder.Services.AddScoped<IHelloWorldService, HelloWorldService>();

var app = builder.Build();

// Configurar middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ColdlineBackEnd API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Middleware de autenticação
app.UseAuthorization();
app.MapControllers();

app.Run();

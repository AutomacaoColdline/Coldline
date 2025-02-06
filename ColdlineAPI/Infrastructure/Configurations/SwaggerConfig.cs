using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;

namespace ColdlineAPI.Infrastructure.Configurations
{
    public static class SwaggerConfig
    {
        public static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ColdlineBackEnd API",
                    Version = "v1",
                    Description = "API para gerenciamento de usuários, processos, máquinas e monitoramento.",
                    Contact = new OpenApiContact
                    {
                        Name = "Equipe Coldline",
                        Email = "suporte@coldline.com",
                        Url = new Uri("https://www.coldline.com")
                    }
                });

                // Configuração para autenticação JWT no Swagger
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
                        new List<string>()
                    }
                });
            });
        }
    }
}

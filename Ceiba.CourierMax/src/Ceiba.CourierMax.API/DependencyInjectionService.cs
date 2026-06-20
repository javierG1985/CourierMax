using Ceiba.CourierMax.API.Services;
using Ceiba.CourierMax.API.Services.Interfaces;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Ceiba.CourierMax.API;

public static class DependencyInjectionService
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Política global de cookies — aplica a todas las cookies de la aplicación
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.HttpOnly            = HttpOnlyPolicy.Always;
            options.Secure              = CookieSecurePolicy.SameAsRequest;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
        });

        // Autenticación JWT — lee el token desde la cookie HttpOnly
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = configuration["Jwt:Issuer"],
                    ValidAudience            = configuration["Jwt:Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        ctx.Token = ctx.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        // Infraestructura HTTP — generación/revocación del JWT en cookie
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // FluentValidation — auto-validación de modelos en el pipeline de ASP.NET
        services.AddFluentValidationAutoValidation();

        // Respuesta estándar ModelResponse cuando falla la validación automática
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = ctx =>
            {
                var errors = ctx.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                var message = string.Join("; ", errors);
                var response = ResponseApiService.Response(StatusCodes.Status400BadRequest, message);
                return new ObjectResult(response) { StatusCode = StatusCodes.Status400BadRequest };
            };
        });

        // Swagger con esquema de cookie
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "CourierMax API", Version = "v1" });

            c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
            {
                Type        = SecuritySchemeType.ApiKey,
                In          = ParameterLocation.Cookie,
                Name        = "access_token",
                Description = "Cookie HttpOnly establecida automáticamente al hacer POST /api/auth/login"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "cookieAuth"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}

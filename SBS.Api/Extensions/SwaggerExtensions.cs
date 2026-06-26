using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

namespace SBS.Api.Extensions;

/// <summary>
/// Cấu hình Swagger UI (JWT Bearer) - do Manager feature team quản lý.
/// Teammate làm Auth KHÔNG cần sửa file này.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "Swimming Booking System API",
                Version     = "v1",
                Description = "API hệ thống đặt lịch bơi. Base URL: /api/manager (Role: Manager)"
            });

            // Khai báo JWT Bearer scheme
            var jwtScheme = new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Nhập token: Bearer {your_token}"
            };

            options.AddSecurityDefinition("Bearer", jwtScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static WebApplication UseSwaggerUI(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            // Gọi phương thức gốc của ASP.NET Core
            Microsoft.AspNetCore.Builder.SwaggerUIBuilderExtensions.UseSwaggerUI(app);
        }
        return app;
    }
}

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SBS.Application.Common.Behaviors;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.CheckIn.Commands;
using SBS.Application.Features.CheckIn.Validators;
using SBS.Infrastructure.Data;
using SBS.Infrastructure.Identity;
using SBS.Infrastructure.Services;
using System.Reflection;

namespace SBS.Api.ServiceExtensions;

public static class StaffServiceExtensions
{
    public static IServiceCollection AddStaffServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Database Context (CQRS: Writer connection dùng cho EF Core - ghi + đọc qua ORM)
        var writerConnection = configuration.GetConnectionString("WriterConnection") 
            ?? throw new InvalidOperationException("Connection string 'WriterConnection' not found in configuration.");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(writerConnection, b => b.MigrationsAssembly("SBS.Infrastructure")));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // 2. ASP.NET Core Identity Core
        services.AddIdentityCore<AppUser>()
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // 3. Application Services
        services.AddScoped<IIdentityService, IdentityService>();

        // 4. MediatR with Pipeline Validation Behavior
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CheckInCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // 5. FluentValidation
        services.AddValidatorsFromAssemblyContaining<CheckInRequestValidator>();

        return services;
    }
}

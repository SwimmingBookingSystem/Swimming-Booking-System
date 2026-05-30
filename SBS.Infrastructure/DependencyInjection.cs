using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SBS.Infrastructure.Data;
using SBS.Infrastructure.Identity;
using System;

using SBS.Application.Common.Interfaces;
using SBS.Infrastructure.Data.Repositories;

namespace SBS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //1.Database Configuration(CQRS Pattern)
        // Write Database Connection(sbs_writer login with full write access)
        services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(configuration.GetConnectionString("WriteConnection")));

        //// Read Database Connection (sbs_reader login with read-only access)
        services.AddDbContext<ReadDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ReadConnection")));

        //Tuấn Anh
        //var connectionString = configuration.GetConnectionString("DefaultConnection");

        //services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        //services.AddDbContext<ReadDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        // 2. Identity Configuration (Bound to Write DB Context)
        services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // 3. Repositories & Unit of Work registration
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReadOnlyUnitOfWork, ReadOnlyUnitOfWork>();

        // 4. Identity & Current User Services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, Services.CurrentUserService>();
        services.AddScoped<IIdentityService, Services.IdentityService>();

        return services;
    }
}

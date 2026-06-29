using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SBS.Infrastructure.Data;
using SBS.Infrastructure.Identity;
using System;
using System.Text;
using System.Threading.Tasks;

using SBS.Application.Common.Interfaces;
using SBS.Infrastructure.Data.Repositories;

namespace SBS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        // 1. Database Configuration (CQRS Pattern)
        // Write Database Connection(sbs_writer login with full write access)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("WriteConnection")));

        // Read Database Connection (sbs_reader login with read-only access)
        services.AddDbContext<ReadDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ReadConnection")));

        // -------- Tuấn Anh  
        //var connectionString = configuration.GetConnectionString("DefaultConnection");

        //services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        //services.AddDbContext<ReadDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        // -------- Tuấn Anh

        // 2. Identity Configuration (Bound to Write DB Context)
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            // Tạm thời tắt các luật mật khẩu phức tạp để tiện test
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 1;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 0;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configure Authentication using JWT
        var secret = configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var issuer = configuration["JwtSettings:Issuer"];
        var audience = configuration["JwtSettings:Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidIssuer = issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            // CẤU HÌNH LÝ THUYẾT COOKIE: Đọc token từ Cookie tên "accessToken"
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey("accessToken"))
                    {
                        context.Token = context.Request.Cookies["accessToken"];
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // 3. Repositories & Unit of Work registration
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReadOnlyUnitOfWork, ReadOnlyUnitOfWork>();

        // 4. Identity & Current User Services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, Services.CurrentUserService>();
        services.AddScoped<IIdentityService, Services.IdentityService>();
        services.AddScoped<IAdminService, Services.AdminService>();
        services.AddScoped<ITokenService, Services.TokenService>();
        services.AddScoped<IAuthService, Services.Auth.AuthService>();
        services.AddScoped<IEmailService, Services.Email.EmailService>();

        // Cloudinary Services
        services.Configure<SBS.Infrastructure.Models.CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
        services.AddScoped<ICloudinaryService, Services.CloudinaryService>();

        // 5. Customer Bookings Infrastructure Services Registration
        services.AddCustomerBookingsInfrastructure(configuration);
        // 5. Staff-specific Services (isolated — no conflict with auth team)
        services.AddScoped<IStaffUserService, Services.StaffUserService>();

        return services;
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using SBS.Api.Middlewares;
using SBS.Application;
using SBS.Infrastructure;
using SBS.Infrastructure.Data;
using SBS.Infrastructure.Data.Seed;
using SBS.Infrastructure.Identity;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// 1. Xử lý cờ lệnh --migrate-only SỚM trước khi đăng ký các dịch vụ nặng khác
if (args.Contains("--migrate-only"))
{
    var connStr = builder.Configuration.GetConnectionString("WriteConnection")
                  ?? builder.Configuration["ConnectionStrings:WriteConnection"];
    Console.WriteLine("[DB Migrator] Starting EF Core migrations...");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connStr));

    var appMigrate = builder.Build();
    using var scope = appMigrate.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    int retries = 10;
    while (retries > 0)
    {
        try
        {
            Console.WriteLine("[DB Migrator] Thực thi EF Core Migrations DDL...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("[DB Migrator] Hoàn tất EF Core Migrations thành công!");
            return; // Kết thúc tiến trình sạch (Exited 0)
        }
        catch (Exception ex)
        {
            retries--;
            Console.WriteLine($"[DB Migrator Warning] Chưa thể kết nối DB ({ex.Message}). Đang thử lại... (còn {retries} lần)");
            if (retries == 0) throw;
            await Task.Delay(3000);
        }
    }
    return;
}

// 2. Cấu hình Trusted Proxy Subnet (172.28.0.0/16) cho Nginx
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("172.28.0.0"), 16));
});

// 3. Health Checks đầy đủ SQL Server, Redis, RabbitMQ
var redisConn = builder.Configuration.GetConnectionString("RedisConnection") ?? "redis:6379";

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("sqlserver", tags: new[] { "ready" })
    .AddRedis(redisConn, name: "redis", tags: new[] { "ready" });

// 4. Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new SBS.Api.Converters.NullableDateOnlyJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// CORS — cho phép SBS.WebApp gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Allow all origins for dev/proxy
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Swimming Booking System API", Version = "v1" });

    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    options.CustomSchemaIds(type => 
    {
        if (!type.IsGenericType)
        {
            return type.FullName?.Replace("+", ".") ?? type.Name;
        }

        var baseName = type.Name.Split('`')[0];
        var genericArgs = string.Join("And", type.GetGenericArguments().Select(t => t.Name));
        return $"{baseName}Of{genericArgs}";
    });

    options.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date" });
    options.MapType<DateOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date", Nullable = true });
    options.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time" });
    options.MapType<TimeOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time", Nullable = true });
    
    options.MapType<TimeSpan>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time-span" });
    options.MapType<TimeSpan?>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time-span", Nullable = true });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register Clean Architecture layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var checks = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Status.ToString());
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(checks));
    }
});

// Seed Data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var uploadSeedImages = app.Configuration.GetValue<bool>("SeedSettings:UploadImagesToCloudinary");
        var cloudinaryService = uploadSeedImages
            ? services.GetRequiredService<SBS.Application.Common.Interfaces.ICloudinaryService>() : null;
        await DataSeeder.SeedDataAsync(context, userManager, roleManager);
        await SBS.Infrastructure.Data.Seed.CustomerSeed.CustomerPoolSeeder.SeedCustomerPoolsAsync(context, cloudinaryService, uploadSeedImages);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seed Warning] Data seeding failed (app continues): {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

var swaggerEnabled = app.Environment.IsDevelopment()
    || app.Configuration.GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
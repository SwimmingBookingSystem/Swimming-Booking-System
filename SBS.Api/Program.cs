using Microsoft.AspNetCore.Identity;
using SBS.Api.Middlewares;
using SBS.Application;
using SBS.Infrastructure;
using SBS.Infrastructure.Data;
using SBS.Infrastructure.Data.Seed;
using SBS.Infrastructure.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
        policy.SetIsOriginAllowed(origin => true) // Allow all origins for dev
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Swimming Booking System API", Version = "v1" });

    // Resolve conflicts khi 2+ controllers cùng base route (ManagerPoolController & ManagerPoolTicketController)
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    // Tránh lỗi trùng SchemaId khi có class cùng tên ở namespace khác và xử lý tên cho generic types (List, v.v.)
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

    // Fix Swagger 500: Lỗi serialize DateOnly/TimeOnly trong Swashbuckle của .NET 8
    options.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date" });
    options.MapType<DateOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date", Nullable = true });
    options.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time" });
    options.MapType<TimeOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time", Nullable = true });
    
    // Fix Swagger 500: TimeSpan cũng có thể gây crash khi sinh Schema nếu không được map thành string
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

// Seed Data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var cloudinaryService = services.GetRequiredService<SBS.Application.Common.Interfaces.ICloudinaryService>();
        await DataSeeder.SeedDataAsync(context, userManager, roleManager);
        await SBS.Infrastructure.Data.Seed.CustomerSeed.CustomerPoolSeeder.SeedCustomerPoolsAsync(context, cloudinaryService);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seed Warning] Data seeding failed (app continues): {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
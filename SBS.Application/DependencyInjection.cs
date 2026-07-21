using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SBS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Register Application Services
        services.AddScoped<SBS.Application.Features.Manager.Services.Interfaces.IPoolManagementService, SBS.Application.Features.Manager.Services.Implementations.PoolManagementService>();
        services.AddScoped<SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService, SBS.Application.Features.Manager.Services.Implementations.SlotManagementService>();
        services.AddScoped<SBS.Application.Features.Manager.Services.Interfaces.ITicketManagementService, SBS.Application.Features.Manager.Services.Implementations.TicketManagementService>();
        services.AddScoped<SBS.Application.Features.Customer_Bookings.Interfaces.IBookingCalculationService, SBS.Application.Features.Customer_Bookings.Services.BookingCalculationService>();

        return services;
    }
}

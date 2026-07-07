using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SBS.Infrastructure;

public static class DependencyInjectionCustomerBookings
{
    public static IServiceCollection AddCustomerBookingsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // MassTransit configuration
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SBS.Application.Features.Customer_Bookings.Consumers.SendBookingConfirmationConsumer>();
            x.AddConsumer<SBS.Application.Features.Customer_Bookings.Consumers.SlotCapacityFreedEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqOptions = configuration.GetSection("RabbitMQ");
                cfg.Host(rabbitMqOptions["Host"] ?? "localhost", rabbitMqOptions["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbitMqOptions["Username"] ?? "guest");
                    h.Password(rabbitMqOptions["Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        // Add Repositories
        services.AddScoped<SBS.Application.Features.Customer_Bookings.Interfaces.IPoolSlotBookingRepository, SBS.Infrastructure.CustomerBookings.Repositories.PoolSlotBookingRepository>();

        // Add PayOS SDK configurations, QRCoder, Email Service here later
        services.AddScoped<SBS.Application.Features.Customer_Bookings.Interfaces.IEmailService, SBS.Infrastructure.CustomerBookings.Services.EmailService>();
        services.AddScoped<SBS.Application.Features.Customer_Bookings.Interfaces.IQRCodeService, SBS.Infrastructure.CustomerBookings.Services.QRCodeService>();
        services.AddScoped<SBS.Application.Features.Customer_Bookings.Interfaces.IPayOSService, SBS.Infrastructure.CustomerBookings.Services.PayOSService>();

        // Register PayOS instance
        services.AddSingleton(provider => 
        {
            var config = provider.GetRequiredService<IConfiguration>().GetSection("PayOS");
            return new Net.payOS.PayOS(config["ClientId"]!, config["ApiKey"]!, config["ChecksumKey"]!);
        });

        // Background worker
        services.AddHostedService<SBS.Infrastructure.CustomerBookings.BackgroundServices.PendingBookingExpirationWorker>();

        return services;
    }
}

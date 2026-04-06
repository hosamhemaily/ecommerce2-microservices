using Application.Interfaces;
using Application.Interfaces.ApplicationServices;
using Infrastructure.PaymentHttp;
using Infrastructure.Persistence;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Messaging.Consumers;
using Refit;    
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool needpayment = false)
    {
        services.AddSingleton<RabbitMqEventBus>();

        services.AddSingleton<IEventBus>(sp =>
        {
            var bus = sp.GetRequiredService<RabbitMqEventBus>();
            bus.InitializeAsync().GetAwaiter().GetResult();
            return bus;
        });

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<OrderCreatedConsumer>();

        if (needpayment)
        {
            services.AddScoped<PaymentResultConsumer>();
            services.AddScoped<InventoryResultConsumer>();

            var paymentBaseUrl = configuration["PaymentApi:BaseUrl"];

            services.AddRefitClient<IPaymentApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(paymentBaseUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    });
        }

        return services;
    }
}
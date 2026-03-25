using Application.Interfaces;
using Application.Interfaces.ApplicationServices;
using Domain.Events;
using Infrastructure.PaymentHttp;
using Infrastructure.Persistence;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
namespace Infrastructure
{
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

            services.AddScoped<IOrderRepository, OrderRepository>();

            // Consumers
            services.AddScoped<Messaging.Consumers.OrderCreatedConsumer>();

            if (needpayment)
            {
                services.AddScoped<Messaging.Consumers.PaymentResultConsumer>();

                var paymentBaseUrl = configuration["PaymentApi:BaseUrl"];

                services.AddRefitClient<IPaymentApi>()
                    .ConfigureHttpClient(c =>
                    {
                        c.BaseAddress = new Uri(paymentBaseUrl);
                    }).ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        }); ;
            }

            var provider = services.BuildServiceProvider();
            var eventBus = provider.GetRequiredService<IEventBus>();

            // OrderCreated subscription
            var orderCreatedConsumer = provider.GetRequiredService<Messaging.Consumers.OrderCreatedConsumer>();
            eventBus.Subscribe<OrderCreatedEvent>(
                "OrderCreated",
                async evt => await orderCreatedConsumer.Consume(evt));

            if (needpayment)
            {
                var paymentConsumer = provider.GetRequiredService<Messaging.Consumers.PaymentResultConsumer>();

                eventBus.Subscribe<PaymentRequestedEvent>(
                    "PaymentRequested",
                    async evt => await paymentConsumer.HandleInitial(evt));

                eventBus.Subscribe<PaymentSucceededEvent>(
                    "PaymentSucceeded",
                    async evt => await paymentConsumer.HandleSuccess(evt));

                eventBus.Subscribe<PaymentFailedEvent>(
                    "PaymentFailed",
                    async evt => await paymentConsumer.HandleFailed(evt));
            }

            return services;
        }
    }
}

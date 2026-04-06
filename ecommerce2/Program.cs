using Application.BackgroundJobs;
using Application.Commands;
using Application.Interfaces;
using Domain.Events;
using Infrastructure;
using Infrastructure.Messaging.Consumers;
using Infrastructure.PaymentHttp;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));
// Add Infrastructure layer services (RabbitMQ, EventBus, etc.)
builder.Services.AddInfrastructure(builder.Configuration,true);


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHostedService<OutboxProcessor>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

   
}

using (var scope = app.Services.CreateScope())
{
    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

    eventBus.Subscribe<OrderCreatedEvent>(
        nameof(OrderCreatedEvent),
        async evt =>
        {
            using var innerScope = app.Services.CreateScope();

            var consumer = innerScope.ServiceProvider
                .GetRequiredService<OrderCreatedConsumer>();

            await consumer.Consume(evt);
        });

eventBus.Subscribe<InventoryRequestedEvent>(
    nameof(InventoryRequestedEvent),
    async evt =>
    {
        using var innerScope = app.Services.CreateScope();

        var consumer = innerScope.ServiceProvider
            .GetRequiredService<InventoryResultConsumer>();

        await consumer.HandleInitial(evt);
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


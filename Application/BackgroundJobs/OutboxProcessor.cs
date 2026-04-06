using Application.Interfaces;
using Application.Interfaces.ApplicationServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.BackgroundJobs
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;


        public OutboxProcessor(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var repo = scope.ServiceProvider
                    .GetRequiredService<IOutboxMessageRepository>();

                var _bus = scope.ServiceProvider
                    .GetRequiredService<IEventBus>();

                var _unitow = scope.ServiceProvider
                    .GetRequiredService<IUnitOfWork>();

                var messages = await repo.GetMessages(stoppingToken);
                foreach (var message in messages)
                {
                    // Deserialize and publish
                    var evt = JsonSerializer.Deserialize<object>(message.Content);
                    await _bus.Publish(message.Type, evt);

                    message.Processed = true;
                    message.ProcessedOn = DateTime.UtcNow;
                }

                await _unitow.SaveChangesAsync(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
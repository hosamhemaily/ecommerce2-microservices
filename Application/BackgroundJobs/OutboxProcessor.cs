using Application.Interfaces;
using Application.Interfaces.ApplicationServices;
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
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly IUnitOfWork _unitow;
        private readonly IEventBus _bus;

        public OutboxProcessor(IOutboxMessageRepository outboxMessageRepository,
            IUnitOfWork unitow,
            IEventBus bus)
        {
            _outboxMessageRepository = outboxMessageRepository;
            _unitow = unitow;   
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var messages = await _outboxMessageRepository.GetMessages(stoppingToken);
                

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
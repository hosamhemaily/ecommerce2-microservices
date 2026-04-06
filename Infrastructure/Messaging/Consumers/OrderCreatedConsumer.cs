using Application.Interfaces;
using Application.Interfaces.ApplicationServices;
using Domain.Entities;
using Domain.Events;
using Infrastructure.Persistence;
using System.Text.Json;

namespace Infrastructure.Messaging.Consumers
{
    public class OrderCreatedConsumer
    {
        private readonly AppDbContext _db;
        private readonly IOutboxMessageRepository _outboxRepo;
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreatedConsumer(
            AppDbContext db,
            IOutboxMessageRepository outboxRepo,
            IUnitOfWork unitOfWork)
        {
            _db = db;
            _outboxRepo = outboxRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(OrderCreatedEvent evt)
        {
            var saga = new OrderSaga
            {
                OrderId = evt.OrderId
            };

            _db.OrderSagas.Add(saga);

            // Write the payment request to the outbox so OutboxProcessor publishes it reliably.
            var paymentRequested = new PaymentRequestedEvent(evt.OrderId, evt.Amount);
            await _outboxRepo.AddAsync(
                new OutboxMessage
                {
                    Content = JsonSerializer.Serialize(paymentRequested),
                    Type = nameof(PaymentRequestedEvent)
                },
                CancellationToken.None);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}

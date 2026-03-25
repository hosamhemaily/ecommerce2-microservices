using Application.Interfaces;
using Domain.Entities;
using Domain.Events;
using Infrastructure.Persistence;

namespace Infrastructure.Messaging.Consumers
{
    public class OrderCreatedConsumer
    {
        private readonly AppDbContext _db;
        private readonly IEventBus _bus;

        public OrderCreatedConsumer(AppDbContext db, IEventBus bus)
        {
            _db = db;
            _bus = bus;
        }

        public async Task Consume(OrderCreatedEvent evt)
        {
            var saga = new OrderSaga
            {
                OrderId = evt.OrderId
            };

            _db.OrderSagas.Add(saga);
            await _db.SaveChangesAsync();

            await _bus.Publish("PaymentRequested",
                new PaymentRequestedEvent(evt.OrderId, evt.Amount));
        }
    }
}

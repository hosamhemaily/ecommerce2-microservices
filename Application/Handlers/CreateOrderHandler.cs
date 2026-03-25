using Application.Commands;
using Application.Interfaces;
using Application.Interfaces.ApplicationServices;
using Domain.Entities;
using Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _db;
        private readonly IEventBus _bus;

        public CreateOrderHandler(IOrderRepository db, IEventBus bus)
        {
            _db = db;
            _bus = bus;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
        {
            var order = Order.Create(request.Amount);

            await _db.AddAsync(order, ct);
            await _db.SaveChangesAsync(ct);

            // 🔥 Start Saga
            await _bus.Publish("OrderCreated", new OrderCreatedEvent(order.guid, order.Amount));

            return order.guid;
        }
    }
}

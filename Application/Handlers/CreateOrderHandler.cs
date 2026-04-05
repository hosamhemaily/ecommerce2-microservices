using Application.Commands;
using Application.Interfaces;
using Application.Interfaces.ApplicationServices;
using Domain.Entities;
using Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _db;
        private readonly IOutboxMessageRepository _outboxRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _bus;

        public CreateOrderHandler(IOrderRepository db, IOutboxMessageRepository outboxRepo,
            IUnitOfWork unitOfWork, IEventBus bus)
        {
            _db = db;
            _bus = bus;
            _outboxRepo = outboxRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
        {
            var order = Order.Create(request.Amount);
            var orderevent = new OrderCreatedEvent(order.guid, order.Amount);

            await _db.AddAsync(order, ct);
            await _outboxRepo.AddAsync(new OutboxMessage {Content = JsonSerializer.Serialize(orderevent),
                Type = nameof(OrderCreatedEvent)
                
            },ct);

            await _unitOfWork.SaveChangesAsync(ct);

            // 🔥 Start Saga
            //await _bus.Publish("OrderCreated", orderevent);

            return order.guid;
        }
    }
}

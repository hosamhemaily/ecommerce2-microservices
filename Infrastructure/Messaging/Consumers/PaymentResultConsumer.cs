using Application.Interfaces.ApplicationServices;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Infrastructure.PaymentHttp;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messaging.Consumers
{
    public class PaymentResultConsumer
    {
        private readonly AppDbContext _db;
        private readonly IPaymentApi _paymentApi;
        private readonly IOutboxMessageRepository _outboxRepo;
        private readonly IUnitOfWork _unitOfWork;
        public PaymentResultConsumer(AppDbContext db,
            IPaymentApi paymentApi,
            IOutboxMessageRepository outboxRepo,
            IUnitOfWork unitOfWork)
        {
            _db = db;
            _paymentApi = paymentApi;
            _outboxRepo = outboxRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleInitial(PaymentRequestedEvent evt)
        {
            try
            {
                
                var result =  await _paymentApi.InitiatePayment(evt.OrderId.ToString());
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }
        public async Task HandleSuccess(PaymentSucceededEvent evt)
        {
            var saga = await  _db.OrderSagas.Where(x=>x.OrderId== evt.OrderId).FirstOrDefaultAsync();

            saga.PaymentCompleted = true;
            var inventoryRequested = new InventoryRequestedEvent(evt.OrderId);
            await _outboxRepo.AddAsync(
                new OutboxMessage
                {
                    Content = JsonSerializer.Serialize(inventoryRequested),
                    Type = nameof( InventoryRequestedEvent)
                },
                CancellationToken.None);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task HandleFailed(PaymentFailedEvent evt)
        {
            var order = await _db.Orders.FindAsync(evt.OrderId);
            order.Status = OrderStatus.Failed;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

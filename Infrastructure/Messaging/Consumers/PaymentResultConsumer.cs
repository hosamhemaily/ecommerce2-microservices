using Application.Interfaces;
using Domain.Enums;
using Domain.Events;
using Infrastructure.PaymentHttp;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        private readonly IEventBus _bus;
        private readonly IPaymentApi _paymentApi;
        public PaymentResultConsumer(AppDbContext db, IEventBus bus,
            IPaymentApi paymentApi)
        {
            _db = db;
            _bus = bus;
            _paymentApi = paymentApi;
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
            await _db.SaveChangesAsync();

            await _bus.Publish("InventoryRequested",
                new InventoryRequestedEvent(evt.OrderId));
        }

        public async Task HandleFailed(PaymentFailedEvent evt)
        {
            var order = await _db.Orders.FindAsync(evt.OrderId);
            order.Status = OrderStatus.Failed;
            await _db.SaveChangesAsync();
        }
    }
}

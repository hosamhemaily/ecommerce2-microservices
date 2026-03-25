using Domain.Enums;
using Domain.Events;
using Infrastructure.PaymentHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messaging.Consumers
{
    public class InventoryResultConsumer
    {
        public InventoryResultConsumer()
        {
                
        }
        public async Task HandleInitial(InventoryRequestedEvent evt)
        {
            

        }
        public async Task HandleSuccess(InventoryRequestedEvent evt)
        {
            
        }

        public async Task HandleFailed(InventoryRequestedEvent evt)
        {
            
        }
    }
}

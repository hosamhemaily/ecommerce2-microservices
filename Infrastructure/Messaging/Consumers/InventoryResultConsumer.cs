using Domain.Enums;
using Domain.Events;
using Infrastructure.PaymentHttp;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Messaging.Consumers
{

    public class InventoryResultConsumer
    {
        private readonly AppDbContext _db;

        public InventoryResultConsumer(AppDbContext db)
        {
            _db = db;
        }
        public async Task HandleInitial(InventoryRequestedEvent evt)
        {
            Console.WriteLine($"Inventory check requested for order {evt.OrderId}");
            //do inventiory check here, for demo we will just randomly decide if inventory is available or not
            if (DateTime.Now.Minute%2 == 0)
            {
                var saga = await _db.OrderSagas.Where(x => x.OrderId == evt.OrderId).FirstOrDefaultAsync();

                saga.InventoryReserved = true;
                await _db.SaveChangesAsync();
            }
            else
            {
                await HandleFailed(evt);
            }
                

        }
      

        public async Task HandleFailed(InventoryRequestedEvent evt)
        {
            const int maxRetries = 3;
            const int delayMilliseconds = 1000;

            int attempt = 0;
            bool success = false;

            while (attempt < maxRetries && !success)
            {
                attempt++;
                Console.WriteLine($"Retrying inventory reservation for order {evt.OrderId}, attempt {attempt}...");

                // Mock retry logic: randomly succeed
                if (DateTime.Now.Second % 2 == 0)
                {
                    var saga = await _db.OrderSagas.Where(x => x.OrderId == evt.OrderId).FirstOrDefaultAsync();
                    if (saga != null)
                    {
                        saga.InventoryReserved = true;
                        await _db.SaveChangesAsync();
                        success = true;
                        Console.WriteLine($"Inventory reservation succeeded on retry {attempt} for order {evt.OrderId}.");
                    }
                }

                if (!success && attempt < maxRetries)
                {
                    await Task.Delay(delayMilliseconds);
                }
            }

            if (!success)
            {
                Console.WriteLine($"Inventory reservation failed after {maxRetries} attempts for order {evt.OrderId}.");
                // Optionally, update saga or notify failure here
            }
        }
    }
}

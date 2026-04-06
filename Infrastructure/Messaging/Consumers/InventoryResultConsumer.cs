using Domain.Enums;
using Domain.Events;
using Infrastructure.InventoryHttp;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Messaging.Consumers
{

    public class InventoryResultConsumer
    {
        private readonly AppDbContext _db;
        private readonly IInventoryApi _inventoryApi;

        public InventoryResultConsumer(AppDbContext db, IInventoryApi inventoryApi)
        {
            _db = db;
            _inventoryApi = inventoryApi;
        }

        public async Task HandleInitial(InventoryRequestedEvent evt)
        {
            Console.WriteLine($"Inventory check requested for order {evt.OrderId}");

            try
            {
                // Demo mapping: reserve 1 unit from a fixed SKU.
                // You can extend InventoryRequestedEvent later to carry sku/quantity.
                await _inventoryApi.AdjustAsync("DEMO-SKU-1", new AdjustInventoryRequest(-1));
                var saga = await _db.OrderSagas.Where(x => x.OrderId == evt.OrderId).FirstOrDefaultAsync();
                if (saga != null)
                {
                    saga.InventoryReserved = true;
                    await _db.SaveChangesAsync();
                }
            }
            catch
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

                try
                {
                    var resultinventoryresponse = await _inventoryApi.AdjustAsync("DEMO-SKU-1", new AdjustInventoryRequest(-1));
                    var saga = await _db.OrderSagas.Where(x => x.OrderId == evt.OrderId).FirstOrDefaultAsync();
                    if (saga != null)
                    {
                        saga.InventoryReserved = true;
                        await _db.SaveChangesAsync();
                        success = true;
                        Console.WriteLine($"Inventory reservation succeeded on retry {attempt} for order {evt.OrderId}.");
                    }
                }
                catch
                {
                    // Keep retry loop behavior for transient failures.
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

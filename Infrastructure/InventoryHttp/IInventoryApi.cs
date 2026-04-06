using Refit;

namespace Infrastructure.InventoryHttp
{
    public interface IInventoryApi
    {
        [Post("/api/inventory/items/{sku}/adjust")]
        Task<InventoryAdjustResponse> AdjustAsync(string sku, [Body] AdjustInventoryRequest body);
    }

    public sealed record AdjustInventoryRequest(int Delta);

    public sealed record InventoryAdjustResponse(string Sku, int QuantityOnHand);
}

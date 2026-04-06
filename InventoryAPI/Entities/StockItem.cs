namespace InventoryAPI.Entities;

public class StockItem
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
}

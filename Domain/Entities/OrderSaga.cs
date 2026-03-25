using Domain.Enums;

namespace Domain.Entities
{
    public class OrderSaga
    {
        public long Id { get; set; }
        public Guid OrderId { get; set; }
        public bool PaymentCompleted { get; set; }
        public bool InventoryReserved { get; set; }
        public string Status { get; set; } = "Pending";

    }
}
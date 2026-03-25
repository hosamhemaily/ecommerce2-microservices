using Domain.Enums;

namespace Domain.Entities
{
    public class Order
    {
        public long Id { get; set; }
        public Guid guid { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public OrderStatus Status { get; set; }

        public static Order Create(decimal amount)
        {
            return new Order
            {
                guid = Guid.NewGuid(),
                Amount = amount,
                Status = OrderStatus.Pending
            };
        }
    }
}
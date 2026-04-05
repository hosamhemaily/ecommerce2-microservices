using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = default!;
        public string Content { get; set; } = default!;
        public bool Processed { get; set; } = false;
        public DateTime? ProcessedOn { get; set; }
    }
}

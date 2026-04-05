using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ApplicationServices
{
    public interface IOutboxMessageRepository
    {
        Task AddAsync(OutboxMessage order, CancellationToken ct);
        Task<List<OutboxMessage>> GetMessages(CancellationToken ct);
    }
}

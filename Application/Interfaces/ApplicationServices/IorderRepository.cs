using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ApplicationServices
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}

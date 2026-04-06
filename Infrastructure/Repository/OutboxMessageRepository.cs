
using Application.Interfaces.ApplicationServices;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Migrations;

namespace Infrastructure.Repository
{
    public class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly AppDbContext _db;
        public OutboxMessageRepository(AppDbContext db)
        {
            _db = db;
            var s =  _db.Database.GetDbConnection().ConnectionString;
        }
       

        public async Task AddAsync(OutboxMessage order, CancellationToken ct)
        {
            await _db.OutboxMessage.AddAsync(order, ct);
        }

        public async Task<List<OutboxMessage>> GetMessages(CancellationToken ct)
        {
            return await _db.OutboxMessage.Where(x=>x.Processed == false).ToListAsync(ct);
        }
    }
}

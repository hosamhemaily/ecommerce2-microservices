using InventoryAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options) { }

    public DbSet<StockItem> StockItems => Set<StockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Sku).IsUnique();
            e.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        });
    }
}

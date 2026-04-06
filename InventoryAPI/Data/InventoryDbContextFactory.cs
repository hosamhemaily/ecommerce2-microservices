using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventoryAPI.Data;

public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer(
                "Server=localhost,11433;Database=InventoryDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True;")
            .Options;
        return new InventoryDbContext(options);
    }
}

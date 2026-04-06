using InventoryAPI.Data;
using InventoryAPI.Entities;
using InventoryAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Inventory")));

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await db.Database.MigrateAsync();
    if (!await db.StockItems.AnyAsync())
    {
        db.StockItems.AddRange(
            new StockItem { Id = Guid.NewGuid(), Sku = "DEMO-SKU-1", QuantityOnHand = 100 },
            new StockItem { Id = Guid.NewGuid(), Sku = "DEMO-SKU-2", QuantityOnHand = 5 });
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var inventory = app.MapGroup("/api/inventory");

inventory.MapGet("/items", async (InventoryDbContext db) =>
    await db.StockItems
        .AsNoTracking()
        .OrderBy(x => x.Sku)
        .Select(x => new { x.Sku, x.QuantityOnHand })
        .ToListAsync());

inventory.MapGet("/items/{sku}", async (string sku, InventoryDbContext db) =>
{
    var item = await db.StockItems.AsNoTracking().FirstOrDefaultAsync(x => x.Sku == sku);
    return item is null ? Results.NotFound() : Results.Ok(new { item.Sku, item.QuantityOnHand });
});

inventory.MapPost("/items/{sku}/adjust", async (string sku, AdjustBody body, InventoryDbContext db) =>
{
    var item = await db.StockItems.FirstOrDefaultAsync(x => x.Sku == sku);
    if (item is null)
        return Results.NotFound();

    var next = item.QuantityOnHand + body.Delta;
    if (next < 0)
        return Results.BadRequest("Insufficient quantity for this adjustment.");

    item.QuantityOnHand = next;
    await db.SaveChangesAsync();
    return Results.Ok(new { item.Sku, item.QuantityOnHand });
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

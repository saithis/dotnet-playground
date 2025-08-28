using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace WolverineApi.Database;

public class Item
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; }
}

public class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This enables your DbContext to map the incoming and
        // outgoing messages as part of the outbox
        modelBuilder.MapWolverineEnvelopeStorage();

        // Your normal EF Core mapping
        modelBuilder.Entity<Item>(map =>
        {
            map.ToTable("items");
            map.HasKey(x => x.Id);
            map.Property(x => x.Name);
        });
    }
}
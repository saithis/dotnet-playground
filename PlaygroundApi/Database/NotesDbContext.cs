using Microsoft.EntityFrameworkCore;
using PlaygroundApi.Database.Entities;
using PlaygroundApi.OutboxPattern;

namespace PlaygroundApi.Database;

public class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options), IOutboxDbContext
{
    public DbSet<Note> Notes { get; set; }
    public ICollection<IMessage> OutboxMessages { get; } = new List<IMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessageEntity>();
    }
}
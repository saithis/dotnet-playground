using Microsoft.EntityFrameworkCore;
using PlaygroundApi.Database.Entities;
using PlaygroundApi.OutboxPattern;

namespace PlaygroundApi.Database;

public class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options), IOutboxDbContext
{
    public DbSet<Note> Notes { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}
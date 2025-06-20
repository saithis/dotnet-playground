using Microsoft.EntityFrameworkCore;
using PlaygroundApi.Database.Entities;

namespace PlaygroundApi.Database;

public class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes { get; set; }

}
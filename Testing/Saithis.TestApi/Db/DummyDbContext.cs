using Microsoft.EntityFrameworkCore;
using Saithis.TestApi.Db.Entities;

namespace Saithis.TestApi.Db;

public class DummyDbContext : DbContext
{
    public DummyDbContext(DbContextOptions<DummyDbContext> options)
        : base(options)
    {
    }
    public DbSet<DummyItem> DummyItems { get; set; }
}

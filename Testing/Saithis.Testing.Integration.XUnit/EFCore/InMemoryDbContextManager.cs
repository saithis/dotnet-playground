
using Microsoft.EntityFrameworkCore;

namespace Saithis.Testing.Integration.XUnit.EFCore;

public class InMemoryDbContextManager<T> : IDbContextManager<T> where T : DbContext
{
    private readonly bool _seedData;
    private DbContextOptions<T>? _dbContextOptions;

    public InMemoryDbContextManager(bool seedData = false)
    {
        _seedData = seedData;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        using T dbContext = CreateDbContext();
        dbContext.Database.EnsureDeleted();
    }

    public T CreateDbContext()
    {
        var createSchema = false;
        if (_dbContextOptions == null)
        {
            createSchema = true;
            _dbContextOptions = new DbContextOptionsBuilder<T>()
                .UseInMemoryDatabase($"{typeof(T)}-{Guid.NewGuid():N}")
                .Options;
        }

        var dbContext = (T) Activator.CreateInstance(
            typeof(T),
            _dbContextOptions
        )!;
        if (_seedData && createSchema)
        {
            dbContext.Database.EnsureCreated();
        }

        return dbContext;
    }

    public void DeleteDatabase()
    {
        _dbContextOptions = null;
    }
}

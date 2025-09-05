
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Saithis.Testing.Integration.XUnit.EFCore;

public class SqlLiteDbContextManager<T> : IDbContextManager<T> where T : DbContext
{
    private SqliteConnection? _dbConnection;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        using T dbContext = CreateDbContext();
        dbContext.Database.EnsureDeleted();
        _dbConnection?.Dispose();
    }

    public T CreateDbContext()
    {
        var createSchema = false;
        if (_dbConnection == null)
        {
            createSchema = true;
            _dbConnection = CreateAndOpenSqliteConnection();
        }

        var dbContext = (T) Activator.CreateInstance(
            typeof(T),
            new DbContextOptionsBuilder<T>().UseSqlite(_dbConnection).Options
        )!;
        if (createSchema)
        {
            dbContext.Database.EnsureCreated();
        }

        return dbContext;
    }

    public void DeleteDatabase()
    {
        _dbConnection?.Dispose();
        _dbConnection = null;
    }

    private static SqliteConnection CreateAndOpenSqliteConnection()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.CreateFunction("NEWID", Guid.NewGuid);
        conn.Open();
        return conn;
    }
}

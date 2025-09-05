
using Microsoft.EntityFrameworkCore;

namespace Saithis.Testing.Integration.XUnit.EFCore;

public interface IDbContextManager<out T> : IDisposable where T : DbContext
{
    T CreateDbContext();
    void DeleteDatabase();
}

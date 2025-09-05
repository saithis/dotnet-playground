
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Saithis.Testing.Integration.XUnit.EFCore;

namespace Saithis.Testing.Integration.XUnit.Service;

public class ServiceScopeContext(IServiceFxiture serviceFixture) : IDisposable
{
    private readonly List<DbContext> _dbContexts = [];
    private IServiceScope? _serviceScope;
    private IServiceScope ServiceScope => _serviceScope ??= serviceFixture.ServiceScopeFactory.CreateScope();
    public IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _dbContexts.ForEach(dbContext => dbContext.Dispose());
        _dbContexts.Clear();
        _serviceScope?.Dispose();
    }

    public async Task DbSaveChangesAllAsync()
    {
        foreach (DbContext dbContext in _dbContexts)
        {
            await dbContext.SaveChangesAsync();
        }
    }

    protected T CreateAndRegisterDbContext<T>(IDbContextManager<T> dbContextManager)
        where T : DbContext
    {
        T dbContext = dbContextManager.CreateDbContext();
        _dbContexts.Add(dbContext);
        return dbContext;
    }
}

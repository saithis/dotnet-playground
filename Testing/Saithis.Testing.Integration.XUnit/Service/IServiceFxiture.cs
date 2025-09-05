
using Microsoft.Extensions.DependencyInjection;

namespace Saithis.Testing.Integration.XUnit.Service;

public interface IServiceFxiture : IDisposable
{
    IServiceScopeFactory ServiceScopeFactory { get; }
    Task SetupTestContextAsync();
    Task CleanupTestContextAsync();
}

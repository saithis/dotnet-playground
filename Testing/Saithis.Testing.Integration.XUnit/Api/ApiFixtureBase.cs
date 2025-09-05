
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Saithis.Testing.Integration.XUnit.Service;
using Xunit.Abstractions;

namespace Saithis.Testing.Integration.XUnit.Api;

public abstract class ApiFixtureBase<TWebApplicationFactory, TEntryPoint> : IServiceFxiture
    where TWebApplicationFactory : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private HttpClient? _clientAnonymous;
    public required TWebApplicationFactory WebApplicationFactory { get; init; }
    public HttpClient ClientAnonymous => _clientAnonymous ??= WebApplicationFactory.Server.CreateClient();

    public ITestOutputHelper? TestOutputHelper { get; set; }

    public IServiceScopeFactory ServiceScopeFactory =>
        WebApplicationFactory.Server.Services.GetRequiredService<IServiceScopeFactory>();

    public abstract Task SetupTestContextAsync();

    public abstract Task CleanupTestContextAsync();

    public virtual void Dispose()
    {
        _clientAnonymous?.Dispose();
        _clientAnonymous = null;
        WebApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    public HttpClient CreateClient() => WebApplicationFactory.Server.CreateClient();
}

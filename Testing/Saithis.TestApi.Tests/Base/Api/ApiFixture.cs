using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Saithis.TestApi.Db;
using Saithis.Testing.Integration.XUnit.Api;
using Saithis.Testing.Integration.XUnit.EFCore;
using TUnit.Core;

namespace Saithis.TestApi.Tests.Base.Api;

// TUnit doesn't use collection definitions like XUnit

public sealed class ApiFixture : ApiFixtureBase<ApiWebApplicationFactory<Program>, Program>
{
    public ApiFixture()
    {
        WebApplicationFactory = new ApiWebApplicationFactory<Program>(
            new ApiWebApplicationFactoryOptions
            {
                ConfigureTestServices = services =>
                {
                    services.AddScoped<TimeProvider>(_ => TimeProviderOverride ?? TimeProvider.System);
                    services.AddScoped(_ => DummyDbContextManager.CreateDbContext());
                },
            });

        ClientNoScope = WebApplicationFactory.Server.CreateClient();
        ClientNoScope.BaseAddress = new Uri("https://localhost/");
        ClientNoScope.DefaultRequestHeaders.Authorization = AuthHeaders.NoScopes;
    }

    public HttpClient ClientNoScope { get; }
    public FakeTimeProvider? TimeProviderOverride { get; set; }

    public IDbContextManager<DummyDbContext> DummyDbContextManager { get; } =
        new SqlLiteDbContextManager<DummyDbContext>();


    public override Task SetupTestContextAsync()
    {
        return Task.CompletedTask;
    }

    public override Task CleanupTestContextAsync()
    {
        TimeProviderOverride = null;
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        ClientNoScope.Dispose();
        base.Dispose();
    }
}

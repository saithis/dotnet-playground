using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Saithis.TestApi.Db;
using Saithis.Testing.Integration.XUnit.Api;
using Saithis.Testing.Integration.XUnit.EFCore;
using Xunit;

namespace Saithis.TestApi.Tests.Base.Api;

[CollectionDefinition(nameof(ApiFixture))]
public class TestWebApiCollectionMarker : ICollectionFixture<ApiFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public sealed class ApiFixture : ApiFixtureBase<ApiWebApplicationFactory<Program>, Program>
{
    public ApiFixture()
    {
        WebApplicationFactory = new ApiWebApplicationFactory<Program>(
            new ApiWebApplicationFactoryOptions
            {
                TestOutputHelper = () => TestOutputHelper,
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

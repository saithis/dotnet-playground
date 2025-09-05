using Saithis.Testing.Integration.XUnit.Api;
using Xunit;
using Xunit.Abstractions;

namespace Saithis.TestApi.Tests.Base.Api;

[Collection(nameof(ApiFixture))]
public class ApiTest : ApiTestBase<ApiFixture, ApiWebApplicationFactory<Program>, Program, ScopeContext>
{
    public ApiTest(ApiFixture fixture, DbResetOptions dbResetOptions, ITestOutputHelper output) : base(fixture, output)
    {
        RegisterDbResetMode(Fixture.DummyDbContextManager, dbResetOptions.Dummy);
    }

    protected override ScopeContext CreateScopeContext() => new(Fixture);
}

using Microsoft.Extensions.Logging;
using Saithis.Testing.Integration.XUnit.Api;
using TUnit.Core;

namespace Saithis.TestApi.Tests.Base.Api;

[ClassDataSource(typeof(ApiFixture), Shared = [SharedType.PerTestSession])]
[NotInParallel]
public class ApiTest : ApiTestBase<ApiFixture, ApiWebApplicationFactory<Program>, Program, ScopeContext>
{
    public ApiTest(ApiFixture fixture, DbResetOptions dbResetOptions) : base(fixture)
    {
        RegisterDbResetMode(Fixture.DummyDbContextManager, dbResetOptions.Dummy);
    }

    protected override ScopeContext CreateScopeContext() => new(Fixture);
}

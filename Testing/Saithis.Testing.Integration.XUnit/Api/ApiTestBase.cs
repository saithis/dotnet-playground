
using Microsoft.AspNetCore.Mvc.Testing;
using Saithis.Testing.Integration.XUnit.Service;
using Xunit.Abstractions;

namespace Saithis.Testing.Integration.XUnit.Api;

public abstract class
    ApiTestBase<TFixture, TWebApplicationFactory, TEntryPoint, TScope> : ServiceTestBase<TFixture, TScope>, IDisposable
    where TFixture : ApiFixtureBase<TWebApplicationFactory, TEntryPoint>, IServiceFxiture
    where TScope : ServiceScopeContext
    where TWebApplicationFactory : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    protected ApiTestBase(TFixture fixture, ITestOutputHelper output)
        : base(fixture)
    {
        Fixture.TestOutputHelper = output;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Fixture.TestOutputHelper = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

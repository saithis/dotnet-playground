
using Microsoft.AspNetCore.Mvc.Testing;
using Saithis.Testing.Integration.XUnit.Service;
using TUnit.Core;

namespace Saithis.Testing.Integration.XUnit.Api;

public abstract class
    ApiTestBase<TFixture, TWebApplicationFactory, TEntryPoint, TScope>(TFixture fixture)
    : ServiceTestBase<TFixture, TScope>(fixture)
    where TFixture : ApiFixtureBase<TWebApplicationFactory, TEntryPoint>, IServiceFxiture
    where TScope : ServiceScopeContext
    where TWebApplicationFactory : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
}

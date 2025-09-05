using Saithis.TestApi.Db;
using Saithis.Testing.Integration.XUnit.Service;

namespace Saithis.TestApi.Tests.Base.Api;

public class ScopeContext(ApiFixture serviceFixture)
    : ServiceScopeContext(serviceFixture)
{
    private DummyDbContext? _dummyDbContext;

    public DummyDbContext DummyDbContext =>
        _dummyDbContext ??= CreateAndRegisterDbContext(serviceFixture.DummyDbContextManager);
}

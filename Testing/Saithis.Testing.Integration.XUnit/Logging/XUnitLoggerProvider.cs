
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Saithis.Testing.Integration.XUnit.Logging;

public sealed class XUnitLoggerProvider(Func<ITestOutputHelper?> testOutputHelper) : ILoggerProvider
{
    private readonly LoggerExternalScopeProvider _scopeProvider = new();

    public ILogger CreateLogger(string categoryName) => new XUnitLogger(testOutputHelper, _scopeProvider, categoryName);

    public void Dispose()
    {
    }
}

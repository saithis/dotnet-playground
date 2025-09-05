
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Saithis.Testing.Integration.XUnit.Api;

public class ApiWebApplicationFactoryOptions
{
    public required Func<ITestOutputHelper?> TestOutputHelper { get; init; }
    public required Action<IServiceCollection> ConfigureTestServices { get; init; }
    public bool SetupMockAuthentication { get; init; } = true;
}

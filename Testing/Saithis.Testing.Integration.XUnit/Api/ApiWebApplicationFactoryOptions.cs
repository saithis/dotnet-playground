
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace Saithis.Testing.Integration.XUnit.Api;

public class ApiWebApplicationFactoryOptions
{
    public required Action<IServiceCollection> ConfigureTestServices { get; init; }
    public bool SetupMockAuthentication { get; init; } = true;
}

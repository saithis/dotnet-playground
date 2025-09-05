
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Saithis.Testing.Integration.XUnit.Api;

public class ApiWebApplicationFactory<TStartup>(ApiWebApplicationFactoryOptions options)
    : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .AddIntegrationTestDefaults(options.SetupMockAuthentication)
            .ConfigureTestServices(options.ConfigureTestServices);
    }
}


using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Saithis.Testing.Integration.XUnit.Api.Auth;

namespace Saithis.Testing.Integration.XUnit.Api;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder AddIntegrationTestDefaults(this IWebHostBuilder builder,
        bool setupMockAuthentication = true)
    {
        builder
            .UseEnvironment("Test")
            .ConfigureAppConfiguration((_, config) =>
                config
                    .AddJsonFile("appsettings.json", false)
                    .AddJsonFile("appsettings.Test.json", true)
                    .AddEnvironmentVariables());

        if (setupMockAuthentication)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(ApiTestAuthHandler.SchemeName)
                    .AddApiTestScheme();
            });
        }

        return builder;
    }
}


using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Saithis.Testing.Integration.XUnit.Api.Auth;
using Saithis.Testing.Integration.XUnit.Logging;
using Xunit.Abstractions;

namespace Saithis.Testing.Integration.XUnit.Api;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder AddIntegrationTestDefaults(
        this IWebHostBuilder builder,
        Func<ITestOutputHelper?> testOutputHelper,
        bool setupMockAuthentication = true)
    {
        builder
            .UseEnvironment("Test")
            .ConfigureAppConfiguration((_, config) =>
                config
                    .AddJsonFile("appsettings.json", false)
                    .AddJsonFile("appsettings.Test.json", true)
                    .AddEnvironmentVariables())
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.Services.AddSingleton<ILoggerProvider>(_ =>
                    new XUnitLoggerProvider(testOutputHelper));
            });

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

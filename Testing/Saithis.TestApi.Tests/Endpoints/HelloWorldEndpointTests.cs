using System.Net;
using AwesomeAssertions;
using Saithis.TestApi.Tests.Base.Api;
using Saithis.Testing.Integration.XUnit.Assertion;
using TUnit.Assertions;
using TUnit.Core;

namespace Saithis.TestApi.Tests.Endpoints;

public class HelloWorldEndpointTests(ApiFixture fixture)
    : ApiTest(fixture, DbResetOptions.None)
{
    [Test]
    public async Task Should_Return_200_With_Hello_World()
    {
        // Arrange
        Console.WriteLine("Starting Hello World test - TUnit captures this output automatically");
        var client = Fixture.CreateClient();

        // Act
        Console.WriteLine("Making HTTP request to localhost");
        var response = await client.GetAsync("http://localhost/");

        // Assert
        Console.WriteLine($"Response status: {response.StatusCode}");
        var body = await ApiAssert.ResponseStringAsync(response, HttpStatusCode.OK);
        body.Should().Be("Hello World!");
        Console.WriteLine("Test completed successfully - this output will be captured by TUnit");
    }
}

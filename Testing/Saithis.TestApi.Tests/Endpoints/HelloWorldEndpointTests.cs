using System.Net;
using AwesomeAssertions;
using Saithis.TestApi.Tests.Base.Api;
using Saithis.Testing.Integration.XUnit.Assertion;
using Xunit;
using Xunit.Abstractions;

namespace Saithis.TestApi.Tests.Endpoints;

public class HelloWorldEndpointTests(ApiFixture fixture, ITestOutputHelper output)
    : ApiTest(fixture, DbResetOptions.None, output)
{
    [Fact]
    public async Task Should_Return_200_With_Hello_World()
    {
        // Arrange
        var client = Fixture.CreateClient();

        // Act
        var response = await client.GetAsync("http://localhost/");

        // Assert
        var body = await ApiAssert.ResponseStringAsync(response, HttpStatusCode.OK);
        body.Should().Be("Hello World!");
    }
}

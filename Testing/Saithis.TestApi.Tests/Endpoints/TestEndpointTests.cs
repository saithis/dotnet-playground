using System.Net;
using Microsoft.EntityFrameworkCore;
using Saithis.TestApi.Db.Entities;
using Saithis.TestApi.Endpoints;
using Saithis.TestApi.Tests.Base.Api;
using Saithis.Testing.Integration.XUnit.Assertion;
using Xunit;
using Xunit.Abstractions;

namespace Saithis.TestApi.Tests.Endpoints;

public class TestEndpointTests(ApiFixture fixture, ITestOutputHelper output)
    : ApiTest(fixture, DbResetOptions.Test, output)
{
    [Fact]
    public async Task Should_Return_404_When_No_Item_Found()
    {
        // Arrange
        HttpClient client = Fixture.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/test");

        // Assert
        await ApiAssert.ProblemResponseAsync(response, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task Should_Overwrite_Name_When_Item_Found()
    {
        // Arrange
        await InScopeAsync(async ctx =>
        {
            ctx.DummyDbContext.DummyItems.Add(new DummyItem { Name = "Some Dummy item" });
            await ctx.DummyDbContext.SaveChangesAsync();
        });
        HttpClient client = Fixture.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/test");

        // Assert
        await InScopeAsync(async ctx =>
        {
            var dto = await ApiAssert.ResponseAsync<TestEndpoint.ResponseDto>(response, HttpStatusCode.OK);
            Assert.Equal("I am module 1", dto!.Message);

            DummyItem dummyItem = await ctx.DummyDbContext.DummyItems.SingleAsync();
            Assert.Equal("Overwritten", dummyItem.Name);
        });
    }
}

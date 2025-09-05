using System.Net;
using Microsoft.EntityFrameworkCore;
using Saithis.TestApi.Db.Entities;
using Saithis.TestApi.Endpoints;
using Saithis.TestApi.Tests.Base.Api;
using Saithis.Testing.Integration.XUnit.Assertion;

namespace Saithis.TestApi.Tests.Endpoints;

public class TestEndpointTests(ApiFixture fixture)
    : ApiTest(fixture, DbResetOptions.Test)
{
    [Test]
    public async Task Should_Return_404_When_No_Item_Found()
    {
        // Arrange
        HttpClient client = Fixture.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/test");

        // Assert
        await ApiAssert.ProblemResponseAsync(response, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Test]
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
            await Assert.That(dto!.Message).IsEqualTo("I am module 1");

            DummyItem dummyItem = await ctx.DummyDbContext.DummyItems.SingleAsync();
            await Assert.That(dummyItem.Name).IsEqualTo("Overwritten");
        });
    }
}

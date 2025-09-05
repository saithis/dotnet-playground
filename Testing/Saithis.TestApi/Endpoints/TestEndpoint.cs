using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saithis.TestApi.Db;

namespace Saithis.TestApi.Endpoints;

public static class TestEndpoint
{
    public static void Register(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/test", Handle);
    }

    private static async Task<Results<Ok<ResponseDto>, ProblemHttpResult>> Handle([FromServices] DummyDbContext dbContext)
    {
        var item = await dbContext.DummyItems.FirstOrDefaultAsync();
        if (item == null)
        {
            return TypedResults.Problem(title: "Item not found", statusCode: StatusCodes.Status404NotFound, type: "NOT_FOUND");
        }

        item.Name = "Overwritten";
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(new ResponseDto
        {
            Message = "I am module 1",
        });
    }

    public record ResponseDto
    {
        public required string Message { get; init; }
    }
}

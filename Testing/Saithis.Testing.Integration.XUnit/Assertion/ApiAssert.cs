
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TUnit.Assertions;
using TUnit.Core;

namespace Saithis.Testing.Integration.XUnit.Assertion;

public static class ApiAssert
{
    public static async Task ProblemDetailsResult(IResult result, int statusCode, string code)
    {
        if (result is not ProblemHttpResult problem)
            throw new Exception($"Expected ProblemHttpResult but got {result.GetType().Name}");
        
        await Assert.That(problem.StatusCode).IsEqualTo(statusCode);
        
        if (problem.ProblemDetails is not ProblemDetails baseProblem)
            throw new Exception($"Expected ProblemDetails but got {problem.ProblemDetails?.GetType().Name ?? "null"}");
            
        await AssertProblemDetails(baseProblem, statusCode, code);
    }

    public static Task<T> OkResult<T>(IResult result)
    {
        if (result is not Ok<T> okResult)
            throw new Exception($"Expected Ok<{typeof(T).Name}> but got {result.GetType().Name}");
        
        if (okResult.Value is not T dto)
            throw new Exception($"Expected {typeof(T).Name} but got {okResult.Value?.GetType().Name ?? "null"}");
            
        return Task.FromResult(dto);
    }

    public static async Task ResponseAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"AssertStatusCode() Failure: Values differ{Environment.NewLine}" +
                             $"Expected: {expected}{Environment.NewLine}Actual: {response.StatusCode}{Environment.NewLine}Body:{body}";
            throw new Exception(message);
        }
    }

    public static async Task<T?> ResponseAsync<T>(HttpResponseMessage response, HttpStatusCode expected)
    {
        await ResponseAsync(response, expected);

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<string> ResponseStringAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        await ResponseAsync(response, expected);

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<ProblemDetails> ProblemResponseAsync(
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string type)
    {
        if (response.StatusCode != statusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"AssertStatusCode() Failure: Values differ{Environment.NewLine}" +
                             $"Expected: {statusCode}{Environment.NewLine}Actual: {response.StatusCode}{Environment.NewLine}Body:{body}";
            throw new Exception(message);
        }

        try
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            await Assert.That(problemDetails).IsNotNull();
            await AssertProblemDetails(problemDetails, (int) statusCode, type);
            return problemDetails;
        }
        catch (Exception e)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"ProblemResponse() Failure: Values differ{Environment.NewLine}" +
                             $"Expected ProblemDetails{Environment.NewLine}Actual Body:{body}";
            throw new Exception(message, e);
        }
    }

    public static async Task<HttpValidationProblemDetails> IsValidationProblemResponseAsync(
        HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"AssertStatusCode() Failure: Values differ{Environment.NewLine}" +
                             $"Expected: {HttpStatusCode.BadRequest}{Environment.NewLine}Actual: {response.StatusCode}{Environment.NewLine}Body:{body}";
            throw new Exception(message);
        }

        try
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
            await Assert.That(problemDetails).IsNotNull();
            await Assert.That(problemDetails.Errors).IsNotEmpty();
            return problemDetails;
        }
        catch (Exception e)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"ProblemResponse() Failure: Values differ{Environment.NewLine}" +
                             $"Expected HttpValidationProblemDetails{Environment.NewLine}Actual Body:{body}";
            throw new Exception(message, e);
        }
    }

    private static async Task AssertProblemDetails(ProblemDetails problem, int statusCode, string type)
    {
        await Assert.That(problem.Status).IsEqualTo(statusCode);
        await Assert.That(problem.Type).IsEqualTo(type);
        await Assert.That(problem.Title).IsNotNull();
        await Assert.That(problem.Title).IsNotEmpty();
    }
}
